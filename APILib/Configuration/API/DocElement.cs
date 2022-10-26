using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace APILib.Configuration.API;

public class DocElement
{
	[JsonProperty("type")] public string Type { get; set; } = "";

	[JsonProperty("name")] public string Name { get; set; } = "";

	[JsonProperty("brief")] public string Brief { get; set; } = "";

	[JsonProperty("description")] public string Description { get; set; } = "";

	[JsonProperty("returnvalues")] public List<DocParam> ReturnValues { get; set; } = new List<DocParam>();

	[JsonProperty("parameters")] public List<DocParam> Parameters { get; set; } = new List<DocParam>();

	[JsonProperty("examples")] public string Examples { get; set; } = "";

	[JsonProperty("replaces")] public string Replaces { get; set; } = "";

	[JsonProperty("error")] public string Error { get; set; } = "";

	[JsonProperty("tparams")] public List<DocParam> TParams { get; set; } = new List<DocParam>();

	[JsonProperty("Members")] public List<DocParam> members { get; set; } = new List<DocParam>();

	[JsonProperty("notes")] public List<string> Notes { get; set; } = new List<string>();


	private static Regex functionNameExtractor = new Regex(@"(?:[\w\d]+\.)?([\w\d]+)");




	private string _methodName;
	public string FunctionName()
	{
		if (!string.IsNullOrWhiteSpace(_methodName))
			return _methodName;
		
		_methodName = Name;
		var match = functionNameExtractor.Match(Name);
		if (match.Success)
			_methodName = match.Groups[1].Value;
		else
		{
			int five = 5;
		}

		return _methodName;
	}
	public bool Generated { get; set; }
}