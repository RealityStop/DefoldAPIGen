using Newtonsoft.Json;

namespace APILib.Configuration;

public class CustomTypeInjectorLoader : IFileLoader<CustomType>
{
	public bool FilterFilename(string file)
	{
		return file.EndsWith(".json");
	}


	public CustomType Parse(string file)
	{
		string input = File.ReadAllText(file);
		
		return JsonConvert.DeserializeObject<CustomType>(input) ?? throw new InvalidOperationException();
	}


	public string APIType => "custom class";


	public static void Save(CustomType newClass)
	{
		int five = 5;
	}
}