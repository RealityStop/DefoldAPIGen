using System.Diagnostics;
using APILib.Analyzers.Artifacts;
using APILib.Configuration.CustomTypes;

namespace APILib.Analyzers.Impls;

public class CustomTypesLoadAndParse :IAnalyzer
{
	public AnalyzerResult Result { get; set; }
	
	public bool RequirementsMet(GenerationState genState)
	{
		if (String.IsNullOrEmpty(genState.Settings.HandlersLocation))
			return false;

		return true;
	}


	public void Execute(GenerationState genState)
	{
		try
		{
			Debug.Assert(genState.Settings.HandlersLocation != null, "generationSettings.HandlersLocation != null");
		
			//Load and inject the API as an artifact, so other analyzers can use it.
			var loader = new FolderLoader(Path.Combine(genState.Settings.HandlersLocation, "types"));
			var handlers = new CustomTypesArtifact(loader.Load(new CustomTypeInjectorLoader(), true));
			genState.Analyzers.Artifacts.AddArtifact(handlers);
			Result = new AnalyzerResult(AnalyzerResultType.Success);
			Result.AddMessage($" {handlers.CustomTypes.Count()} types loaded");
			
		}
		catch (Exception e)
		{
			Result = new AnalyzerResult(AnalyzerResultType.Fail);
			Result.Fail(e);
			Console.WriteLine(e);
			throw;
		}
		
	}


	public void Reset(GenerationState genState)
	{
		Result = new AnalyzerResult(AnalyzerResultType.NotRun);
	}
}