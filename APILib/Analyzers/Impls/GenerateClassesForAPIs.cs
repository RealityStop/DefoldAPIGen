using System.Text;
using System.Text.RegularExpressions;
using APILib.API;
using APILib.Artifacts;
using APILib.Configuration;
using APILib.Configuration.Handlers;
using APILib.Generators;
using APILib.Helpers;

namespace APILib.Analyzers;

public class GenerateClassesForAPIs : IAnalyzer
{
	public AnalyzerResult Result { get; set; }
	
	
	private IEnumerable<TrimmedAPIArtifact> _trimmedAPIs;
	private CustomHandlersArtifact _customHandlers;
	private CustomTypesArtifact _customTypes;

	//Because variants match against all permutations, they have to list all permutations inside of themselves and only process once.
	private HashSet<string> _handledVariants = new HashSet<string>();
	
	
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<TrimmedAPIArtifact>().Any() &&
		       genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().Any() &&
		       genState.Analyzers.Artifacts.Of<CustomTypesArtifact>().Any();
	}

	
	

	public void Execute(GenerationState genState)
	{
		_trimmedAPIs = genState.Analyzers.Artifacts.Of<TrimmedAPIArtifact>();
		_customHandlers = genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().First();
		_customTypes = genState.Analyzers.Artifacts.Of<CustomTypesArtifact>().First();

		foreach (var trimmedAPI in _trimmedAPIs)
		{
			foreach (var nameSpace in trimmedAPI.Namespaces())
			{
				GeneratedClass generatedClass = new GeneratedClass(nameSpace.Info.Namespace, true)
				{
					Comment = nameSpace.Info.Brief
				};

				_ = _customHandlers.TryFetchHandling(nameSpace.Info.Namespace, out var handler);
				
				
				GenerateMethods(nameSpace, generatedClass, handler);
				
				genState.Analyzers.Artifacts.AddArtifact(generatedClass);
			}
		}

		genState.Analyzers.Artifacts.AddArtifact(new MethodsGeneratedArtifact());
		
		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}




	private void GenerateMethods(DocJson nameSpace, GeneratedClass generatedClass, CustomHandler customHandler)
	{
		foreach (var function in nameSpace.Functions())
		{
			if (ShouldIgnoreMethod(function, customHandler))
			{
				function.Generated = true;
				continue;
			}

			bool skip = false;
			OverrideMethod overrideMethod = null;
			if (customHandler?.Overrides?.TryFetchOverrideForFunction(function, out overrideMethod) ?? false)
			{
				if (overrideMethod.Variants?.Any() ?? false)
				{
					skip = GenerateVariantMethods(generatedClass, function, overrideMethod);
					
					if (skip)
						continue;
					function.Generated = true;
					continue;
				}
			}
			
			skip = !GenerateStandardMethods(generatedClass, function, overrideMethod);
			if (skip)
				continue;
			function.Generated = true;
		}
	}


	private bool GenerateStandardMethods(GeneratedClass generatedClass, DocElement function,
		OverrideMethod overrideMethod)
	{
		List<MethodParam> parameterOptions = new List<MethodParam>();
		List<MethodParam> returnValueOptions = new List<MethodParam>();

		List<DocParam> sourceParameterList = function.Parameters;
		List<DocParam> sourceReturnList = function.ReturnValues;
		
		if (overrideMethod != null)
		{
			 sourceParameterList = overrideMethod.Parameters ?? new List<DocParam>();
			sourceReturnList = overrideMethod.ReturnValues ?? new List<DocParam>();
		}

		bool success = true;
		foreach (var parameter in sourceParameterList)
		{
			if (GenerateMethodParameter(parameter, out var methodParam))
				parameterOptions.Add(methodParam);
			else
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {generatedClass.ClassName}.{function.Name} because of unhandled parameter type '{string.Join(",", parameter.AllTypes())}'");
				success = false;
			}
		}
			
		foreach (var returnValue in sourceReturnList)
		{
			if (GenerateMethodParameter(returnValue, out var methodParam))
				returnValueOptions.Add(methodParam);
			else
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {generatedClass.ClassName}.{function.Name} because of unhandled return type '{string.Join(",", returnValue.AllTypes())}'");
				success = false;
			}
		}
		generatedClass.Methods.Add(new GeneratedMethod(function.FunctionName(), parameterOptions, returnValueOptions){Comment = function.Description});

		return success;
	}


	private bool GenerateVariantMethods(GeneratedClass generatedClass, DocElement function,
		OverrideMethod overrideMethod)
	{
		bool success = true;

		if (_handledVariants.Contains(FormatVariantHandleName(generatedClass, function)))
			return true;

		foreach (var overrideMethodVariant in overrideMethod.Variants)
		{
			List<MethodParam> parameterOptions = new List<MethodParam>();
			List<MethodParam> returnValueOptions = new List<MethodParam>();
			bool skip = false;
			foreach (var parameter in overrideMethodVariant.Parameters)
			{
				if (GenerateMethodParameter(parameter, out var methodParam))
					parameterOptions.Add(methodParam);
				else
				{
					ServiceContainer.Get<IOutput>()
						.WriteLine(
							$"Unable to convert {generatedClass.ClassName}.{function.Name} because of unhandled parameter type '{string.Join(",", parameter.AllTypes())}'");
					skip = true;
				}
			}
			
			foreach (var returnValue in overrideMethodVariant.ReturnValues)
			{
				if (GenerateMethodParameter(returnValue, out var methodParam))
					returnValueOptions.Add(methodParam);
				else
				{
					ServiceContainer.Get<IOutput>()
						.WriteLine(
							$"Unable to convert {generatedClass.ClassName}.{function.Name} because of unhandled return type '{string.Join(",", returnValue.AllTypes())}'");
					skip = true;
				}
			}
			generatedClass.Methods.Add(new GeneratedMethod(function.FunctionName(), parameterOptions, returnValueOptions){Comment = function.Description});

			if (skip)
				success = false;
		}

		_handledVariants.Add(FormatVariantHandleName(generatedClass, function));
		return !success;
	}


	private static Regex functionParameterExtractor = new Regex(@"function\((.*)\)");
	private static Regex functionParameterSeparator = new Regex(@"([\w\d]+)\s+([\w\d]+)");
	private bool GenerateMethodParameter(DocParam parmDocumentation, out MethodParam methodParam)
	{
		methodParam = new MethodParam(parmDocumentation.name, parmDocumentation.doc);
		
		foreach (var type in parmDocumentation.AllTypes())
		{
			if (type.StartsWith("function", StringComparison.OrdinalIgnoreCase))
			{
				if (!GenerateFunctionParameter(type, out var functionParam))
					return false;
				
				methodParam.AddOption(functionParam);
			}
			else
			{
				
				
				var parmOptions = CoveringTypes(type).ToArray();
				foreach (var customType in parmOptions)
				{
					methodParam.AddOption(new CustomTypeGeneratedParam(customType));
				}
			}
		}

		return true;
	}


	private bool GenerateFunctionParameter(string type, out FunctionPointerGeneratedParam functionParam)
	{
		functionParam = default;
		
		var match = functionParameterExtractor.Match(type);
		if (match.Success)
		{
			string extractedFunctionParameters = match.Groups[1].Value;
			var parameterMatches = functionParameterSeparator.Matches(extractedFunctionParameters)
				.Select(x => (x.Groups[1].Value, x.Groups[2].Value));


			List<MethodParam> parms = new List<MethodParam>();

			foreach (var parameterMatch in parameterMatches)
			{
				
				var parameterTypes = CoveringTypes(parameterMatch.Item1).ToArray();
				if (parameterTypes.Count() != 1)
					throw new InvalidOperationException();

				var parameterName = parameterMatch.Item2;

				var methodParam = new MethodParam(parameterName, "");
				foreach (var customType in parameterTypes)
					methodParam.AddOption(new CustomTypeGeneratedParam(customType));
				parms.Add(methodParam);
			}

			functionParam = new FunctionPointerGeneratedParam(parms.ToArray());
			return true;
		}

		return false;
	}


	private string FormatVariantHandleName(GeneratedClass generatedClass, DocElement function)
	{
		return $"{generatedClass.ClassName}.{function.Name}";
	}

	
	private bool ShouldIgnoreMethod(DocElement function, CustomHandler customHandler)
	{
		if (customHandler?.Overrides?.TryFetchOverrideForFunction(function, out var overrideMethod) ?? false)
		{
			return overrideMethod.Ignore;
		}

		return false;
	}
	

	/*private void GenerateParameterizedMethods(GeneratedClass generatedClass, List<MethodParam> finalizedReturnValueOptions,
		DocElement function, List<MethodParam> finalizedParameterOptions)
	{
		var coreReturnValue = finalizedReturnValueOptions.First();
		var outParams = finalizedReturnValueOptions.Skip(1);

		foreach (var returnOptions in coreReturnValue.Options)
		{
			StringBuilder specificationBuilder = new StringBuilder();
			specificationBuilder.Append("public ");
			if (generatedClass.IsStatic)
				specificationBuilder.Append("static ");
			specificationBuilder.Append("extern ");
			if (returnOptions is CustomVoidParam)
				specificationBuilder.Append("void");
			else
				specificationBuilder.Append(coreReturnValue.Name);

			List<GeneratedMethod> methods = new List<GeneratedMethod>();
			FormatParameters(function.FunctionName(), methods, specificationBuilder.ToString(), finalizedParameterOptions,
				new CustomType[0], outParams, new CustomType[0]);
			foreach (var method in methods)
			{
				method.Comment = function.Brief;
			}

			generatedClass.Methods.AddRange(methods);
		}
	}*/




	/*private bool FetchParametersAndReturnValues(DocJson nameSpace,
		DocElement function, CustomHandler customHandler, out IEnumerable<GeneratedParam>[] finalizedParameterOptions,
		out IEnumerable<GeneratedParam>[] finalizedReturnValueOptions)
	{
		bool skip = false;
		finalizedParameterOptions = default;
		finalizedReturnValueOptions = default;
		
		
		List<IEnumerable<GeneratedParam>> parameterOptions = new List<IEnumerable<GeneratedParam>>();
		List<IEnumerable<GeneratedParam>> returnValueOptions = new List<IEnumerable<GeneratedParam>>();

		
		
		//Check for overrides to the parameters
		bool hasCustomParameters = false;
		bool hasCustomReturnValues = false;
		if (customHandler?.Overrides?.TryFetchOverrideForFunction(function, out var overrideMethod) ?? false)
		{
			if (overrideMethod.Parameters != null)
			{
				hasCustomParameters = true;
				skip |= FunctionOverrideParameters(nameSpace, function, overrideMethod, out parameterOptions);
			}

			if (overrideMethod.ReturnValues != null)
			{
				hasCustomReturnValues = true;
				skip |= FunctionOverrideReturnValues(nameSpace, function, overrideMethod, out returnValueOptions);
			}
		}

		if (!hasCustomParameters)
			skip |= FunctionStandardParameters(nameSpace, function, out parameterOptions);

		if (!hasCustomReturnValues)
			skip |= FunctionStandardReturnValues(nameSpace, function, out returnValueOptions);

		if (skip)
			return true;

		finalizedParameterOptions = parameterOptions.ToArray();
		finalizedReturnValueOptions = returnValueOptions.DefaultIfEmpty(new CustomType[] { CustomType.Void }).ToArray();
		return false;
	}*/

	
	/*private bool FunctionStandardParameters(DocJson nameSpace, DocElement function, out List<IEnumerable<GeneratedParam>> parameterOptions)
	{
		bool skip = false;
		parameterOptions = new List<IEnumerable<GeneratedParam>>();
		foreach (var parameter in function.Parameters)
		{
			var coveringTypes = parameter.AllTypes().Select(x => CoveringTypes(x)).ToArray();
			if (coveringTypes.Any(x => !x.Any()))
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {nameSpace.Info.Namespace}.{function.Name} because of unhandled parameter type '{string.Join(",", parameter.AllTypes())}'");
				skip = true;
			}
			else
			{
				parameterOptions.Add(coveringTypes.SelectMany(x => x));
			}
		}

		return skip;
	}*/



	/*private bool FunctionOverrideParameters(DocJson nameSpace, DocElement function, OverrideMethod overrideMethod,
		out List<IEnumerable<CustomType>> returnValueOptions)
	{
		bool skip = false;
		returnValueOptions = new List<IEnumerable<CustomType>>();
		foreach (var returnValue in overrideMethod.Parameters)
		{
			var coveringTypes = returnValue.AllTypes().Select(x => CoveringTypes(x)).ToArray();
			if (coveringTypes.Any(x => !x.Any()))
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {nameSpace.Info.Namespace}.{function.Name} because of unhandled parameter type `{string.Join(",", returnValue.AllTypes())}`");
				skip = true;
			}
			else
			{
				returnValueOptions.Add(coveringTypes.SelectMany(x => x));
			}
		}

		return skip;
	}*/
	

	


	/*private bool FunctionStandardReturnValues(DocJson nameSpace, DocElement function, out List<IEnumerable<CustomType>> returnValueOptions)
	{
		bool skip = false;
		returnValueOptions = new List<IEnumerable<CustomType>>();
		foreach (var returnValue in function.ReturnValues)
		{
			var coveringTypes = returnValue.AllTypes().Select(x => CoveringTypes(x)).ToArray();
			if (coveringTypes.Any(x => !x.Any()))
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {nameSpace.Info.Namespace}.{function.Name} because of unhandled return value '{string.Join(",", returnValue.AllTypes())}'");
				skip = true;
			}
			else
			{
				returnValueOptions.Add(coveringTypes.SelectMany(x => x));
			}
		}

		return skip;
	}*/
	
	/*private bool FunctionOverrideReturnValues(DocJson nameSpace, DocElement function, OverrideMethod overrideMethod,
		out List<IEnumerable<CustomType>> returnValues)
	{
		bool skip = false;
		
		
		returnValues = new List<IEnumerable<CustomType>>();
		foreach (var parameter in overrideMethod.ReturnValues)
		{
			var coveringTypes = parameter.AllTypes().Select(x => CoveringTypes(x)).ToArray();
			if (coveringTypes.Any(x => !x.Any()))
			{
				ServiceContainer.Get<IOutput>()
					.WriteLine(
						$"Unable to convert {nameSpace.Info.Namespace}.{function.Name} because of unhandled return value '{string.Join(",", parameter.AllTypes())}'");
				skip = true;
			}
			else
			{
				returnValues.Add(coveringTypes.SelectMany(x => x));
			}
		}

		return skip;
	}*/
	
	
	
	

	/*private void FormatParameters(
		string methodName,
		List<GeneratedMethod> methods,
		string baseSpecification,
		IEnumerable<MethodParam> parameterOptions,
		IEnumerable<MethodParam> parametersSoFar,
		IEnumerable<MethodParam> outParams,
		IEnumerable<MethodParam> outParametersSoFar)
	{
		var parameterOptionsEnumerated = parameterOptions as IEnumerable<MethodParam>[] ?? parameterOptions.ToArray();
		var outParamsEnumerated = outParams as IEnumerable<MethodParam>[] ?? outParams.ToArray();
		var iteratingList = parameterOptionsEnumerated.DefaultIfEmpty(new CustomTypeGeneratedParam[]{new CustomTypeGeneratedParam(CustomType.Void)}).First();
		var remainingParameters = parameterOptionsEnumerated.Skip(1).ToArray();

		
		foreach (var parameter in iteratingList)
		{
			IEnumerable<GeneratedParam> parameterList = parametersSoFar;
			if (parameter is not CustomVoidParam)
				parameterList = parametersSoFar.Append(parameter);
		
			if (remainingParameters.Any())
			{
				FormatParameters(methodName, methods, baseSpecification, remainingParameters, parameterList, outParamsEnumerated, outParametersSoFar);				
			}
			else
			{
				if (outParamsEnumerated.Any())
				{
					FormatOutParameters(methodName, methods, baseSpecification, remainingParameters, parameterList, outParamsEnumerated, outParametersSoFar);
				}
				else
				{
					methods.Add(new GeneratedMethod(methodName, baseSpecification, parameterList.Where(x=>!x.Name.Equals("void", StringComparison.OrdinalIgnoreCase)), outParametersSoFar));
				}
			}
		}

	}*/


	/*private void FormatOutParameters(
		string methodName,
		List<GeneratedMethod> methods,
		string baseSpecification,
		IEnumerable<IEnumerable<GeneratedParam>> parameterOptions,
		IEnumerable<GeneratedParam> parametersSoFar,
		IEnumerable<IEnumerable<GeneratedParam>> outParams,
		IEnumerable<GeneratedParam> outParametersSoFar)
	{
		var outParamsEnumerated = outParams as IEnumerable<GeneratedParam>[] ?? outParams.ToArray();
		var iteratingList = outParamsEnumerated.First();
		var remainingParameters = outParamsEnumerated.Skip(1).ToArray();

		foreach (var parameter in iteratingList)
		{
			outParametersSoFar = outParametersSoFar.Append(parameter);

			if (remainingParameters.Any())
			{
				FormatOutParameters(methodName, methods, baseSpecification, parameterOptions, parametersSoFar, remainingParameters, outParametersSoFar);
			}
			else
			{
				methods.Add(new GeneratedMethod(methodName, baseSpecification, parametersSoFar, outParametersSoFar));
			}
		}
	}*/


	// static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
	// {
	// 	if (length == 1) return list.Select(t => new T[] { t });
	//
	// 	return GetPermutations(list, length - 1)
	// 		.SelectMany(t => list.Where(e => !t.Contains(e)),
	// 			(t1, t2) => t1.Concat(new T[] { t2 }));
	// }
	

	private IEnumerable<CustomType> CoveringTypes(string name)
	{
		if (string.IsNullOrEmpty(name))
			return Enumerable.Empty<CustomType>();

		return _customTypes.CustomTypes.Where(x =>
		{
			return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
			       x.Implements.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
		});
	}


	public void Reset(GenerationState genState)
	{
	}
}