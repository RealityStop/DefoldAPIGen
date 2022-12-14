using APILib.Analyzers.Artifacts;
using APILib.Analyzers.Generators;

namespace APILib.Analyzers.Impls;

public class GenerateFilesForEachClass : IAnalyzer
{
	private string _outputFolder;
	public AnalyzerResult Result { get; set; }
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<GeneratedClass>().Any();
	}


	public void Execute(GenerationState genState)
	{
		var generatedClasses = genState.Analyzers.Artifacts.Of<GeneratedClass>();

		
		foreach (var generatedClass in generatedClasses)
		{
			string output = GeneratedClassRenderer.Render(generatedClass);
			var outputFile = Path.Combine(_outputFolder, $"{generatedClass.LuaAPIName}.cs");
			File.WriteAllText(outputFile, output);
		}

		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}


	public void Reset(GenerationState genState)
	{
		_outputFolder = genState.Settings.OutputLocation;
	}
}