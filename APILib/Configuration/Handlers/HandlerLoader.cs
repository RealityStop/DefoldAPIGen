using Newtonsoft.Json;

namespace APILib.Configuration.Handlers;

public class HandlerLoader: IFileLoader<Handler>
{
	public bool FilterFilename(string file)
	{
		return file.EndsWith(".json");
	}


	public Handler Parse(string file)
	{
		string input = File.ReadAllText(file);
		
		return JsonConvert.DeserializeObject<Handler>(input) ?? throw new InvalidOperationException();
	}


	public string APIType => "custom handlers";
}