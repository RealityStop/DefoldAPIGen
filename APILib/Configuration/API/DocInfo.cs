using Newtonsoft.Json;

namespace APILib.Configuration.API;

public class DocInfo
{
	[JsonProperty("namespace")] public string Namespace { get; set; } = "";

	[JsonProperty("name")] public string Name { get; set; } = "";

	[JsonProperty("brief")] public string Brief { get; set; } = "";

	[JsonProperty("description")] public string Description { get; set; } = "";

	[JsonProperty("path")] public string Path { get; set; } = "";

	[JsonProperty("file")] public string File { get; set; } = "";

	[JsonProperty("group")] public string Group { get; set; } = "";
}