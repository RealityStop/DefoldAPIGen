using APILib.Configuration.API;
using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class HandlerOverrideMethod
{
	public int TargetParameterCount { get; set; }
	public int TargetReturnValueCount { get; set; }
	
	public bool Ignore { get; set; }
	[JsonProperty("parameters")]
	public List<DocParam> Parameters { get; set; }
	[JsonProperty("returnvalues")]
	public List<DocParam> ReturnValues { get; set; }

	public List<HandlerOverrideMethodVariant> Variants { get; set; }

}