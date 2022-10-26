namespace APILib.Configuration.CustomTypes;

public class CustomLuaHandling
{
	public enum HandlingType{None, Ignore, Template}

	public HandlingType Handling { get; set; } = HandlingType.None;


	public string Template { get; set; } = "";

}