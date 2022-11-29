namespace APILib.Configuration.CustomTypes;

public class CustomTypeDefinition
{
	public static CustomTypeDefinition Void { get; } = new CustomTypeDefinition() { Name = "void" }; 
	
	public string Name { get; set; } = "";
	public string Filename { get; set; } = "";

	public bool Generate { get; set; } = true;

	public CustomTypeDirectionRestriction DirectionRestriction { get; set; } = CustomTypeDirectionRestriction.All;
	
	public CustomSpecification Specification { get; set; }
	public List<string> Implements { get; set; } = new List<string>();
	
	
	public CustomLuaHandling LuaHandling { get; set; }
		
}