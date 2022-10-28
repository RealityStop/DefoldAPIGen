using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class Handler
{
	[JsonProperty("namespace")] public string Namespace { get; set; }
	[JsonProperty("ignore")] public bool Ignore { get; set; }

	[JsonProperty] public string ClassName { get; set; }


	[JsonProperty] public string BaseClass { get; set; }


	[JsonProperty] public string CustomContent { get; set; }


	public HandlerOverrides Overrides { get; set; }
}