namespace APILib.API;

public class DocParam
{
	public string name { get; set; } = "";
	public string doc { get; set; } = "";
	public List<string> types { get; set; } = new List<string>();
	public string type { get; set; } = "";


	public Dictionary<string, string> SpecialTypeComments = new Dictionary<string, string>();

	
	public IEnumerable<string> AllTypes()
	{
		if (!types.Any())
			return new string[]{type};
		else if (string.IsNullOrWhiteSpace(type))
			return types;
		else
			return types.Concat(new string[] { type });
	}
}