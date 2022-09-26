using APILib.API;
using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class OverrideMethod
{
	public int TargetParameterCount { get; set; }
	public int TargetReturnValueCount { get; set; }
	
	public bool Ignore { get; set; }
	[JsonProperty("parameters")]
	public List<DocParam> Parameters { get; set; }
	[JsonProperty("returnvalues")]
	public List<DocParam> ReturnValues { get; set; }

	public List<OverrideMethodVariant> Variants { get; set; }

}