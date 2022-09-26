using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class HandlerLoader: IFileLoader<CustomHandler>
{
	public bool FilterFilename(string file)
	{
		return file.EndsWith(".json");
	}


	public CustomHandler Parse(string file)
	{
		string input = File.ReadAllText(file);
		
		return JsonConvert.DeserializeObject<CustomHandler>(input) ?? throw new InvalidOperationException();
	}


	public string APIType => "custom handlers";
}