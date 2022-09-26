using Newtonsoft.Json;

namespace APILib.Configuration;

public class CustomConstructor
{
	[JsonProperty("parameters")]
	public List<CustomParameter> Parameters { get; set; } = new List<CustomParameter>();

}