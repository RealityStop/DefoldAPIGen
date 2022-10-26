using APILib.Configuration.API;
using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class HandlerOverrideMethodVariant
{
	[JsonProperty("parameters")]
	public List<DocParam> Parameters { get; set; }
	[JsonProperty("returnvalues")]
	public List<DocParam> ReturnValues { get; set; }
}