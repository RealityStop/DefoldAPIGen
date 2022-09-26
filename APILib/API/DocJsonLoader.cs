namespace APILib.API;

public class DocJsonLoader : IFileLoader<DocJson>
{
	public bool FilterFilename(string file)
	{
		return file.EndsWith(".json");
	}


	public DocJson Parse(string file)
	{
		string input = File.ReadAllText(file);
		return DocJson.Parse(input);
	}


	public string APIType => "doc";
}