using System.Reactive.Disposables;
using System.Text;
using APILib.Analyzers.Artifacts;
using APILib.Helpers;

namespace APILib.Analyzers.Generators;

public static  class GeneratedClassRenderer
{
	
	public static string Render(GeneratedClass targetClass)
	{
		FormattedStringBuilder builder = new FormattedStringBuilder();
		using (WriteInitialPreamble(builder))
		{
			using (WriteClassSpecification(builder, targetClass))
			{
				builder.AppendLine("#region Defold API");
				
				WriteMessages(builder, targetClass);
				WriteMethods(builder, targetClass);
				
				builder.AppendLine("#endregion Defold API");

				WriteCustomContent(builder, targetClass);

			}
		}

		return builder.RenderToString();
	}



	private static IDisposable WriteInitialPreamble(FormattedStringBuilder builder)
	{
		builder.AppendLine("using System;");
		builder.AppendLine("using support;");
		builder.AppendLine("using types;");
		builder.AppendLine("");
		
		return Disposable.Empty;
	}
	
	private static IDisposable WriteClassSpecification(FormattedStringBuilder builder, GeneratedClass generatedClass)
	{
		builder.AppendLine("/// <summary>");
		if (!string.IsNullOrWhiteSpace(generatedClass.Comment))
		{
			builder.AppendLine($"/// {generatedClass.Comment}");
			builder.AppendLine("/// ");
		}
		builder.AppendLine("/// </summary>");
		builder.AppendLine($"public {(generatedClass.IsStatic? "static " : "")}class {generatedClass.ClassName.First().ToString().ToUpper() + generatedClass.ClassName.Substring(1)}{(string.IsNullOrWhiteSpace(generatedClass.BaseClass)?"":" : " + generatedClass.BaseClass)}");
		
		return builder.Scope();
	}
	
	

	private static void WriteMessages(FormattedStringBuilder builder, GeneratedClass generatedClass)
	{
		foreach (var generatedMessage in generatedClass.Messages)
		{
			builder.AppendLine("/// <summary>");
			if (!string.IsNullOrWhiteSpace(generatedMessage.Comment))
			{
				builder.AppendLine($"/// {generatedMessage.Comment}");
				builder.AppendLine("/// ");
			}
			builder.AppendLine("/// </summary>");
			builder.AppendLine($"public class {generatedMessage.MessageName}_message : MessageImplementation");


			using (builder.Scope())
			{
				builder.AppendLine($"public static Hash __CODE__ = Defold.hash(\"{generatedMessage.MessageName}\");");
				builder.AppendLine($"public override Hash FetchCode() => __CODE__;");
				builder.AppendLine("");
				
				foreach (var field in generatedMessage.Fields)	
				{
					builder.AppendLine($"public {RenderParamOptionType(field.Options.First())} {field.Name};");
				}
			}
			
			builder.AppendLine("");
			builder.AppendLine("");
		}
	}

	
	

	private static void WriteMethods(FormattedStringBuilder builder, GeneratedClass generatedClass)
	{
		foreach (var generatedMethod in generatedClass.Methods)
		{
			foreach (var returnvaluesOption in generatedMethod.Returnvalues.Options)
			{
				RenderPermutations(generatedClass, generatedMethod, returnvaluesOption, builder, generatedMethod.Parameters,
					new IMethodParamOption[0], generatedMethod.OutParameters, new IMethodParamOption[0]);
			}
		}
	}
	
	
	
	private static void WriteCustomContent(FormattedStringBuilder builder, GeneratedClass targetClass)
	{
		if (string.IsNullOrEmpty(targetClass.CustomContent))
			return;

		var lines = targetClass.CustomContent.Split(new char[] { '\n' });
			//.Select(x => x.Trim(new char[]{' '}));

		foreach (var line in lines)
		{
			builder.AppendLine(line);
		}
	}



	private static void RenderPermutations(
		GeneratedClass generatedClass,
		GeneratedMethod generatedMethod,
		IMethodParamOption returnValue,
		FormattedStringBuilder builder,
		IEnumerable<MethodParam> parameterOptions,
		IEnumerable<IMethodParamOption> parametersSoFar,
		IEnumerable<MethodParam> outParams,
		IEnumerable<IMethodParamOption> outParametersSoFar)
	{
		var parameterOptionsEnumerated = parameterOptions as MethodParam[] ?? parameterOptions.ToArray();;
		var outParamsEnumerated = outParams;
		var currentParam = parameterOptionsEnumerated.DefaultIfEmpty(new MethodParam("void", "").AddOption(new CustomVoidParam())).First();
		var remainingParameters = parameterOptionsEnumerated.Skip(1).ToArray();

		//If the current parmeter is optional, then we need to render the method without this parameter, in addition to any options.
		if (currentParam.IsOptional)
		{
			RenderMethod(generatedClass, generatedMethod, returnValue, builder, parametersSoFar, outParametersSoFar);
		}
		

		foreach (var parameter in currentParam.Options)
		{
			IEnumerable<IMethodParamOption> parameterList = parametersSoFar;
			if (parameter is not CustomVoidParam &&
			    !(parameter is CustomTypeGeneratedParam ctgp && ctgp.Types.Name.Equals("void", StringComparison.OrdinalIgnoreCase ) ) )
				parameterList = parametersSoFar.Append(parameter);

			if (remainingParameters.Any())
			{
				RenderPermutations(generatedClass, generatedMethod, returnValue, builder, remainingParameters, parameterList,
					outParamsEnumerated, outParametersSoFar);
			}
			else
			{
				if (outParamsEnumerated.Any())
				{
					RenderOutPermutations(generatedClass, generatedMethod, returnValue, builder, remainingParameters, parameterList,
						outParamsEnumerated, outParametersSoFar);
				}
				else
				{
					RenderMethod(generatedClass, generatedMethod, returnValue, builder, parameterList, outParametersSoFar);
				}
			}
		}
	}


	private static void RenderMethod(GeneratedClass generatedClass, GeneratedMethod generatedMethod, IMethodParamOption returnValue, FormattedStringBuilder builder, IEnumerable<IMethodParamOption> parametersSoFar, IEnumerable<IMethodParamOption> outParametersSoFar)
	{
		//TODO: actually do the spec.
		builder.AppendLine("/// <summary>");
		FormatComment(builder, generatedMethod.Comment);
		builder.AppendLine("/// ");
		builder.AppendLine($"/// @CSharpLua.Template = \"{FormatTemplate(generatedClass,generatedMethod, parametersSoFar)}\"");
		builder.AppendLine("/// </summary>");

		
			
		//build the specification
		StringBuilder specificationBuilder = new StringBuilder();
		specificationBuilder.Append("public ");
		specificationBuilder.Append("static ");
		specificationBuilder.Append("extern ");
			specificationBuilder.Append(RenderParamOptionType(returnValue));

			
				
		builder.AppendLine($"{specificationBuilder.ToString()} {generatedMethod.MethodName}({FormatParameters(generatedMethod, parametersSoFar, outParametersSoFar)});");

			
		builder.AppendLine("");
		builder.AppendLine("");
	}


	private static char[] splitters = new char[] { '\r', '\n' };
	private static void FormatComment(FormattedStringBuilder builder, string comment)
	{
		var split = comment.Split(splitters, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		foreach (var commentLine in split)
		{
			builder.AppendLine($"/// {commentLine}");
		}
	}


	private static void RenderOutPermutations(
		GeneratedClass generatedClass,
		GeneratedMethod generatedMethod,
		IMethodParamOption returnValue,
		FormattedStringBuilder builder,
		IEnumerable<MethodParam> parameterOptions,
		IEnumerable<IMethodParamOption> parametersSoFar,
		IEnumerable<MethodParam> outParams,
		IEnumerable<IMethodParamOption> outParametersSoFar)
	{
		var outParamsEnumerated = outParams as MethodParam[] ?? outParams.ToArray();
		var currentOutParam = outParamsEnumerated.First();
		var remainingParameters = outParamsEnumerated.Skip(1).ToArray();

		//If the current parmeter is optional, then we need to render the method without this parameter, in addition to any options.
		if (currentOutParam.IsOptional)
		{
			RenderMethod(generatedClass, generatedMethod, returnValue, builder, parametersSoFar, outParametersSoFar);
		}
		
		foreach (var parameter in currentOutParam.Options)
		{
			IEnumerable<IMethodParamOption> outParameterList = outParametersSoFar;
			if (parameter is not CustomVoidParam)
				outParameterList = outParametersSoFar.Append(parameter);

			if (remainingParameters.Any())
			{
				RenderOutPermutations(generatedClass, generatedMethod, returnValue, builder, parameterOptions, parametersSoFar, remainingParameters, outParameterList);
			}
			else
			{
				RenderMethod(generatedClass, generatedMethod, returnValue, builder, parametersSoFar, outParameterList);
			}
		}
	}

	

	private static string RenderParamOptionType(IMethodParamOption returnvaluesOption)
	{
		if (returnvaluesOption is CustomTypeGeneratedParam customParam)
		{
			return customParam.Types.Name;
		}
		else if (returnvaluesOption is FunctionPointerGeneratedParam functionParam)
		{
			string paramList = string.Join(",", functionParam.Params.Select(x => RenderParamOptionType(x.Options.First())));

			return $"Action<{paramList}>";
		}
		else if (returnvaluesOption is CustomVoidParam voidParam)
		{
			return "void";
		}

		throw new NotImplementedException();
		return "unknown";
	}


	private static string FormatTemplate(GeneratedClass generatedClass, GeneratedMethod generatedMethod, IEnumerable<IMethodParamOption> parametersSoFar)
	{
		int count = 0;

		var templateParameters = string.Join(", ", parametersSoFar.Select(x =>"{" +  $"{count++}" + "}"));
		
		return $"{generatedClass.ClassName}.{generatedMethod.MethodName}({templateParameters})";
	}


	private static string FormatParameters(GeneratedMethod generatedMethod, IEnumerable<IMethodParamOption> parametersSoFar, IEnumerable<IMethodParamOption> outParametersSoFar)
	{
		int paramCount = 0;
		int outparamCount = 0;
		
		var parameters = parametersSoFar.Select(x => $"{RenderParamOptionType(x)} {generatedMethod.Parameters.ElementAt(paramCount++).Name}_p{paramCount}").ToArray();
		var outparameters = outParametersSoFar.Select(x => $"out {RenderParamOptionType(x)} {generatedMethod.OutParameters.ElementAt(outparamCount++).Name}_o{outparamCount}").ToArray();

		string parameterString = string.Join(", ", parameters);
		if (parameters.Any() && outparameters.Any())
			parameterString += ", ";
		parameterString += string.Join(", ", outparameters);

		return parameterString;
	}
}