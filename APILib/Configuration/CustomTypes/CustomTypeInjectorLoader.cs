using Newtonsoft.Json;

namespace APILib.Configuration.CustomTypes;

public class CustomTypeInjectorLoader : IFileLoader<CustomTypeDefinition>
{
	public bool FilterFilename(string file)
	{
		return file.EndsWith(".json");
	}


	public CustomTypeDefinition Parse(string file)
	{
		string input = File.ReadAllText(file);
		
		return JsonConvert.DeserializeObject<CustomTypeDefinition>(input) ?? throw new InvalidOperationException();
	}


	public string APIType => "custom class";


	public static void Save(CustomTypeDefinition newClass)
	{
		int five = 5;
	}
}