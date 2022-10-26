using System.Text.RegularExpressions;
using APILib.Configuration.CustomTypes;

namespace APILib.Analyzers.Generators;

public class MethodParam
{
	private static Regex optionalParameterDetectorRegex = new Regex(@"\[[\w-]+\]");

	
	public string Name { get; }
	public string Documentation { get; }
	public bool IsOptional { get; set; }

	public List<IMethodParamOption> Options { get; } = new List<IMethodParamOption>();


	public MethodParam(string name, string documentation)
	{
		IsOptional = optionalParameterDetectorRegex.IsMatch(name);
		Name = Sanitize(name);
		Documentation = documentation;
	}


	private char[] trimchars = new char[] { '[', ']' };
	private string Sanitize(string name)
	{
		name = name.Trim(trimchars);

		return Regex.Replace(name, @"[\[\]\(\)-]", "_");
	}


	public MethodParam AddOption(IMethodParamOption option)
	{
		//Don't add double optional.
		if (IsOptional)
		{
			if (option is CustomVoidParam)
				return this;
			if (option is CustomTypeGeneratedParam param)
			{
				if (param.Types.Name.Equals("nil") || param.Types.Name.Equals("void"))
					return this;
			}
		}

		Options.Add(option);
		return this;
	}
}

public interface IMethodParamOption
{
}

public class CustomVoidParam : IMethodParamOption
{
	public CustomVoidParam()
	{
	}
}

public class CustomTypeGeneratedParam : IMethodParamOption
{
	public CustomTypeGeneratedParam(CustomTypeDefinition customTypesForOption)
	{
		Types = customTypesForOption;
	}


	public CustomTypeDefinition Types { get; }
}

public class FunctionPointerGeneratedParam : IMethodParamOption
{
	public FunctionPointerGeneratedParam(MethodParam[] functionParmList)
	{
		Params = functionParmList;
	}


	public MethodParam[] Params { get; }
}