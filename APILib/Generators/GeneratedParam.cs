using System.Text.RegularExpressions;
using APILib.Configuration;

namespace APILib.Generators;

public class MethodParam
{
	public string Name { get; }
	public string Documentation { get; }

	public List<IMethodParamOption> Options { get; } = new List<IMethodParamOption>();


	public MethodParam(string name, string documentation)
	{
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
	public CustomTypeGeneratedParam(CustomType customTypesForOption)
	{
		Types = customTypesForOption;
	}


	public CustomType Types { get; }
}

public class FunctionPointerGeneratedParam : IMethodParamOption
{
	public FunctionPointerGeneratedParam(MethodParam[] functionParmList)
	{
		Params = functionParmList;
	}


	public MethodParam[] Params { get; }
}