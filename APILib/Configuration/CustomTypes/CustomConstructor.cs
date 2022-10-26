using Newtonsoft.Json;

namespace APILib.Configuration.CustomTypes;

public class CustomConstructor
{
	[JsonProperty("parameters")]
	public List<CustomParameter> Parameters { get; set; } = new List<CustomParameter>();

}