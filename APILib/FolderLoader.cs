namespace APILib;

public class FolderLoader
{
	private readonly string _folder;


	public FolderLoader(string folder)
	{
		_folder = folder;
	}


	public IEnumerable<T> Load<T>(IFileLoader<T> loader, bool loadsubfolderFiles = false)
	{
		var sourceFiles = Directory.EnumerateFiles(_folder, "*", enumerationOptions: new EnumerationOptions() {RecurseSubdirectories = loadsubfolderFiles}).Where(loader.FilterFilename);
		var enumerable = sourceFiles as string[] ?? sourceFiles.ToArray();
		Console.WriteLine($"Found {enumerable.Count()} {loader.APIType} files");

		var parsedResults = enumerable.Select(x =>
			{
				Console.WriteLine($"Reading {x}...");
				return loader.Parse(x);
			})
			.Where(x=>x != null);

		var results = parsedResults as T[] ?? parsedResults.ToArray();
		Console.WriteLine($"Imported {results.Count()} {loader.APIType} files");
		return results;
	}

}