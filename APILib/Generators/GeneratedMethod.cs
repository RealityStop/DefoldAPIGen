using APILib.Configuration;

namespace APILib.Generators;

public class GeneratedMethod
{
	public GeneratedMethod(string methodName, IEnumerable<MethodParam> parameters, IEnumerable<MethodParam> returnValues)
	{
		MethodName = methodName;
		Parameters = parameters.ToArray();
		returnValues = returnValues.DefaultIfEmpty(new MethodParam("", "").AddOption(new CustomVoidParam()));
		
		Returnvalues = returnValues.First();
		OutParameters = returnValues.Skip(1).ToArray();
	}

	public string MethodName { get; }

	public CustomLuaHandling Handling { get; set; }
	public string Comment { get; set; }


	public MethodParam[] Parameters { get; }
	public MethodParam[] OutParameters { get; }
	
	public MethodParam Returnvalues { get; }
	 
}