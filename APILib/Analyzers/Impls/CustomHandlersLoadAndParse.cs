using System.Diagnostics;
using APILib.Analyzers.Artifacts;
using APILib.Configuration.Handlers;

namespace APILib.Analyzers.Impls;

public class CustomHandlersLoadAndParse :IAnalyzer
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
			var loader = new FolderLoader(Path.Combine(genState.Settings.HandlersLocation, "handlers"));
			var handlers = loader.Load(new HandlerLoader(), true);
			var artifact = new CustomHandlersArtifact(handlers.ToDictionary(x=>x.Namespace,x=>x));
			genState.Analyzers.Artifacts.AddArtifact(artifact);
			Result = new AnalyzerResult(AnalyzerResultType.Success);
			Result.AddMessage($" {artifact.Handling.Count()} handlers loaded");
			
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