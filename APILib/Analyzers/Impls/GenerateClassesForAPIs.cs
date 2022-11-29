using System.Text.RegularExpressions;
using APILib.Analyzers.Artifacts;
using APILib.Analyzers.Generators;
using APILib.Configuration.API;
using APILib.Configuration.CustomTypes;
using APILib.Configuration.Handlers;
using APILib.Helpers;

namespace APILib.Analyzers.Impls;

public enum Direction
{
	Parameter,
	Return
}


public class GenerateClassesForAPIs : IAnalyzer
{
	public AnalyzerResult Result { get; set; }
	
	
	private IEnumerable<TrimmedAPIArtifact> _trimmedAPIs;
	private CustomHandlersArtifact _customHandlers;
	private CustomTypesArtifact _customTypes;
	private IOutput _output;

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
		_output = ServiceContainer.Get<IOutput>();
		;

		foreach (var trimmedAPI in _trimmedAPIs)
		{
			foreach (var nameSpace in trimmedAPI.Namespaces())
			{
				GeneratedClass generatedClass = new GeneratedClass(nameSpace.Info.Namespace, true)
				{
					Comment = nameSpace.Info.Brief
				};

				_ = _customHandlers.TryFetchHandling(nameSpace.Info.Namespace, out var handler);

				if (handler != null)
				{
					if (!string.IsNullOrEmpty(handler.ClassName))
					{
						generatedClass.ClassName = handler.ClassName;
					}
					
					if (!string.IsNullOrEmpty(handler.BaseClass))
					{
						generatedClass.IsStatic = false;
						generatedClass.BaseClass = handler.BaseClass;
					}

					if (!string.IsNullOrEmpty(handler.CustomContent))
					{
						generatedClass.CustomContent = handler.CustomContent;
					}
				}

				GenerateMethods(nameSpace, generatedClass, handler);
				GenerateMessages(nameSpace, generatedClass, handler);
				
				genState.Analyzers.Artifacts.AddArtifact(generatedClass);
			}
		}

		genState.Analyzers.Artifacts.AddArtifact(new MethodsGeneratedArtifact());
		
		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}



	private void GenerateMethods(DocJson nameSpace, GeneratedClass generatedClass, Handler handler)
	{
		foreach (var function in nameSpace.Functions())
		{
			if (ShouldIgnoreMethod(function, handler))
			{
				function.Generated = true;
				continue;
			}

			bool skip = false;
			HandlerOverrideMethod overrideMethod = null;
			if (handler?.Overrides?.TryFetchOverrideForFunction(function, out overrideMethod) ?? false)
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
		HandlerOverrideMethod overrideMethod)
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
			if (GenerateMethodParameter(parameter, out var methodParam, Direction.Parameter))
				parameterOptions.Add(methodParam);
			else
			{
				_output
					.WriteLine(
						$"Unable to convert {generatedClass.LuaAPIName}.{function.Name} because of unhandled parameter type '{string.Join(",", parameter.AllTypes())}'");
				success = false;
			}
		}
			
		foreach (var returnValue in sourceReturnList)
		{
			if (GenerateMethodParameter(returnValue, out var methodParam, Direction.Return))
				returnValueOptions.Add(methodParam);
			else
			{
				_output
					.WriteLine(
						$"Unable to convert {generatedClass.LuaAPIName}.{function.Name} because of unhandled return type '{string.Join(",", returnValue.AllTypes())}'");
				success = false;
			}
		}
		generatedClass.Methods.Add(new GeneratedMethod(function.FunctionName(), parameterOptions, returnValueOptions){Comment = function.Description});

		return success;
	}


	private bool GenerateVariantMethods(GeneratedClass generatedClass, DocElement function,
		HandlerOverrideMethod overrideMethod)
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
				if (GenerateMethodParameter(parameter, out var methodParam, Direction.Parameter))
					parameterOptions.Add(methodParam);
				else
				{
					_output
						.WriteLine(
							$"Unable to convert {generatedClass.LuaAPIName}.{function.Name} because of unhandled parameter type '{string.Join(",", parameter.AllTypes())}'");
					skip = true;
				}
			}
			
			foreach (var returnValue in overrideMethodVariant.ReturnValues)
			{
				if (GenerateMethodParameter(returnValue, out var methodParam, Direction.Return))
					returnValueOptions.Add(methodParam);
				else
				{
					_output
						.WriteLine(
							$"Unable to convert {generatedClass.LuaAPIName}.{function.Name} because of unhandled return type '{string.Join(",", returnValue.AllTypes())}'");
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
	


	private bool GenerateMethodParameter(DocParam parmDocumentation, out MethodParam methodParam, Direction direction)
	{
		methodParam = new MethodParam(parmDocumentation.name, parmDocumentation.doc);
		
		foreach (var type in parmDocumentation.AllTypes())
		{
			if (type.StartsWith("literal:", StringComparison.OrdinalIgnoreCase))
			{
				var literalType = type.Substring("literal:".Length);
				methodParam.AddOption(new LiteralGeneratedParam(literalType));
			}
			else if (type.StartsWith("function", StringComparison.OrdinalIgnoreCase))
			{
				if (!GenerateFunctionParameter(type, out var functionParam))
					return false;
				
				methodParam.AddOption(functionParam);
			}
			else
			{
				var parmOptions = CoveringTypes(type, direction).ToArray();
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
				
				var parameterTypes = CoveringTypes(parameterMatch.Item1, Direction.Return).ToArray();
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

	
	private bool ShouldIgnoreMethod(DocElement function, Handler handler)
	{
		if (handler?.Overrides?.TryFetchOverrideForFunction(function, out var overrideMethod) ?? false)
		{
			return overrideMethod.Ignore;
		}

		return false;
	}
	

	
	private void GenerateMessages(DocJson nameSpace, GeneratedClass generatedClass, Handler outValue)
	{
		foreach (var message in nameSpace.Messages())
		{
			bool allParamsValid = true;

			var paramList = message.Parameters.Select(x =>
			{
				var coveringTypes = x.AllTypes().SelectMany(y => CoveringTypes(y, Direction.Parameter));
				if (coveringTypes.Count() != 1)
				{
					_output.WriteLine(
						$"Unable to generate message {nameSpace.Info.Name}.{message.Name} due to unhandled parameter type {x.type}");
					allParamsValid = false;
					return null;
				}

				var methodparm = new MethodParam(x.name, x.doc);
				foreach (var coveringType in coveringTypes)
				{
					methodparm.AddOption(new CustomTypeGeneratedParam(coveringType));
				}

				return methodparm;
			})
				.Where(x=>x!=null);


			if (!allParamsValid)
				continue;



			var generatedMessage = new GeneratedMessage(message.Name, paramList.ToArray());
			generatedClass.Messages.Add(generatedMessage);
			
			message.Generated = true;
		}
	}

	
	

	private IEnumerable<CustomTypeDefinition> CoveringTypes(string name, Direction direction)
	{
		if (string.IsNullOrEmpty(name))
			return Enumerable.Empty<CustomTypeDefinition>();

		return _customTypes.CustomTypes.Where(x =>
		{
			if (!(x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
			    x.Implements.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase))))
				return false;
			
			if (x.DirectionRestriction != CustomTypeDirectionRestriction.All)
			{
				if (x.DirectionRestriction == CustomTypeDirectionRestriction.ParameterOnly &&
				    direction != Direction.Parameter)
					return false;

				if (x.DirectionRestriction == CustomTypeDirectionRestriction.ReturnOnly &&
				    direction != Direction.Return)
					return false;
			}

			return true;
		});
	}


	public void Reset(GenerationState genState)
	{
	}
}