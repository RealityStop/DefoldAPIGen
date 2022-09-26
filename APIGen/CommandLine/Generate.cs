using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using APILib;
using APILib.API;
using APILib.Configuration;
using CommandLine;

namespace APIGen.CommandLine;


[Verb("generate", HelpText = "attempts to generate from a specification")]
public class Generate : IExecutable
{
	[Option("apilocation", Required = true)]
	public string APILocation { get; set; } = "";


	[Option("handlerslocation", Required = true)]
	public string HandlersLocation { get; set; } = "";


	[Option("output", Required = true)] public string OutputDirectory { get; set; } = "";


	public bool Run()
	{
		Console.WriteLine("Executing Generation.");
		
		if (!CheckInputs()) return false;

		FolderLoader docFolder = new FolderLoader(APILocation);
		var docjsons = docFolder.Load(new DocJsonLoader());
		
		
		FolderLoader customFolder = new FolderLoader(APILocation);
		var customHandlers = customFolder.Load(new CustomTypeInjectorLoader());
		
		foreach (var docjson in docjsons)
		{
			WriteConversion(docjson, customHandlers);
		}
		
		return true;
	}


	private IEnumerable<T> Load<T>(string folder, string apiType, Func<string, T> parser)
	{
		var sourceFiles = Directory.EnumerateFiles(folder, "*.json");
		Console.WriteLine($"Found {sourceFiles.Count()} {apiType} files");

		var parsedResults = sourceFiles.Select(x =>
		{
			Console.WriteLine($"Reading {x}...");
			return parser(x);
		})
			.Where(x=>x != null);
		
		Console.WriteLine($"Imported {parsedResults.Count()} {apiType} files");
		return parsedResults;
	}
	

	private bool CheckInputs()
	{
		if (string.IsNullOrEmpty(APILocation))
		{
			Console.WriteLine("--location is empty.");
			return false;
		}

		if (string.IsNullOrEmpty(OutputDirectory))
		{
			Console.WriteLine("--output is empty.");
			return false;
		}

		if (!Directory.Exists(APILocation))
		{
			Console.WriteLine("--location contains no parseable files.");
			return false;
		}

		return true;
	}


	private void WriteConversion(DocJson docjson, IEnumerable<CustomType> customHandlers)
	{
		string outputFile = Path.Combine(OutputDirectory, $"{docjson.Info.Namespace}.cs");

		using (StreamWriter sw = new StreamWriter(outputFile))
		{
			sw.WriteLine("////////////////////////////////////////////////////////////////////////////////");
			sw.WriteLine("// Auto-generated.  Modifications to this file will be lost when regenerated. //");
			sw.WriteLine("////////////////////////////////////////////////////////////////////////////////");
			sw.WriteLine("");
			sw.WriteLine("/*");
			sw.WriteLine($"\t{docjson.Info.Namespace}");
			sw.WriteLine($"\t{docjson.Info.Name}");
			sw.WriteLine($"\t{FormatDescription(docjson)}");
			sw.WriteLine("*/");
			
			sw.WriteLine($"public static class {ReplaceInvalidCharacters(docjson.Info.Namespace)}");
			sw.WriteLine("{");

			WriteStaticMethods(docjson, sw);
			
			
			
			sw.WriteLine("}");
		}
	}


	private void WriteStaticMethods(DocJson docjson, StreamWriter sw)
	{
		
	}


	private static Regex firstCharacterMatcher = new Regex("^[^a-zA-Z_]");
	private static Regex trailingCharacterMatcher = new Regex("[^a-zA-Z0-9_]");
	private string ReplaceInvalidCharacters(string infoNamespace)
	{
		infoNamespace = firstCharacterMatcher.Replace(infoNamespace, "_");
		return trailingCharacterMatcher.Replace(infoNamespace, "_");
	}


	static Regex markupMatcher = new Regex("<.+?>");
	private static string FormatDescription(DocJson docjson)
	{
		return markupMatcher.Replace(docjson.Info.Description, "").Replace("\n","\n\t");
	}
}