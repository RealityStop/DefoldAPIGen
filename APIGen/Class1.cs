using System.Reflection;
using APIGen.CommandLine;
using CommandLine;

namespace APIGen;
public static class MainProgram
{
	//load all types using Reflection
	private static Type[] LoadVerbs()
	{
		return Assembly.GetExecutingAssembly().GetTypes()
			.Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
	}
	
	static void Main(string[] args)
	{

		var commandLineResult = Parser.Default.ParseArguments(args, LoadVerbs())
			.WithParsed(Run)
			.WithNotParsed(HandleErrors);
	}
	
	
	

	private static void Run(object obj)
	{
		if (obj is IExecutable exec)
			exec.Run();
		Console.WriteLine("Success:");
	}

	private static void HandleErrors(IEnumerable<Error> obj)
	{
		var enumerable = obj as Error[] ?? obj.ToArray();
		if (enumerable?.Count() == 1)
		{
			if (!(enumerable.First() is HelpRequestedError)
			    && !(enumerable.First() is VersionRequestedError))
			{
				Console.WriteLine("Failed:");
				foreach (var error in obj)
				{
					Console.WriteLine(error);
				}
				Environment.ExitCode = -1;
			}
		}
	}
}
