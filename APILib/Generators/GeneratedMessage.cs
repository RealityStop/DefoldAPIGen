namespace APILib.Generators;

public class GeneratedMessage
{
	public GeneratedMessage(string name, MethodParam[] fields)
	{
		MessageName = name;
		Fields = fields;
	}

	public string MessageName { get; }

	public string Comment { get; set; }
	public MethodParam[] Fields { get; }
}