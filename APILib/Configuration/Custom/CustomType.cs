using Newtonsoft.Json;

namespace APILib.Configuration;

public class CustomType
{
	public static CustomType Void { get; } = new CustomType() { Name = "void" }; 
	
	public string Name { get; set; } = "";
	public string Filename { get; set; } = "";

	public CustomSpecification Specification { get; set; }
	public List<string> Implements { get; set; } = new List<string>();
	
	
	public CustomLuaHandling Handling { get; set; }
}