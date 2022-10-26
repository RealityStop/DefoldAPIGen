using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using APILib;
using APILib.Configuration;
using APILib.Configuration.API;
using APILib.Configuration.CustomTypes;
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
		

		return true;
	}

}