using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class CustomHandler
{
	[JsonProperty("namespace")]
	public string Namespace { get; set; }
	[JsonProperty("ignore")]
	public bool Ignore { get; set; }


	public Overrides Overrides { get; set; }
}