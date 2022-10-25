using System.Collections;
using Newtonsoft.Json;

namespace APILib.API;

public class DocJson
{
	[JsonProperty("elements")] public List<DocElement> Elements { get; set; } = new List<DocElement>();


	[JsonProperty("info")] public DocInfo Info { get; set; } = new DocInfo();


	public static DocJson Parse(string input)
	{
		return JsonConvert.DeserializeObject<DocJson>(input) ?? throw new InvalidOperationException();
	}


	public IEnumerable<DocElement> Functions()
	{
		return Elements.Where(x =>
			x.Type.Equals("function", StringComparison.OrdinalIgnoreCase));
	}
	
	public IEnumerable<DocElement> Messages()
	{
		return Elements.Where(x =>
			x.Type.Equals("message", StringComparison.OrdinalIgnoreCase));
	}
}