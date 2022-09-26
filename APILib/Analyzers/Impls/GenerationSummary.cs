using APILib.API;
using APILib.Artifacts;
using APILib.Helpers;

namespace APILib.Analyzers;

public class GenerationSummary : IAnalyzer
{
	public class GenerationResult
	{
		public int handled = 0;
		public int total = 0;
	}
	
	public AnalyzerResult Result { get; set; }
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<MethodsGeneratedArtifact>().Any();
	}


	public void Execute(GenerationState genState)
	{
		var trimmedAPIs = genState.Analyzers.Artifacts.Of<TrimmedAPIArtifact>();

		var Output = ServiceContainer.Get<IOutput>();
		Dictionary<string, GenerationResult> elementsHandling = new Dictionary<string, GenerationResult>();

		foreach (var trimmedAPIArtifact in trimmedAPIs)
		{
			foreach (var nameSpace in trimmedAPIArtifact.Namespaces())
			{
				foreach (var element in nameSpace.Elements)
				{
					string elementName = element.Type.ToLowerInvariant();
					if (!elementsHandling.TryGetValue(elementName, out var handling))
					{
						handling = new GenerationResult();
						elementsHandling[elementName] = handling;
					}

					if (element.Generated)
						handling.handled++;
					else
						Output.WriteLine($"{nameSpace.Info.Namespace} - {element.Type} {element.Name} not handled");
					handling.total++;
				}
			}
		}
		
		
		ServiceContainer.Get<IOutput>().WriteLine("Generation Summary:");
		int totalHandledOverall = 0;
		int totalOverall = 0;
		foreach (var generationResult in elementsHandling)
		{
			totalHandledOverall += generationResult.Value.handled;
			totalOverall += generationResult.Value.total;
			ServiceContainer.Get<IOutput>().WriteLine($"{generationResult.Value.handled}/{generationResult.Value.total} {generationResult.Key} handled: {(100.0*generationResult.Value.handled/generationResult.Value.total).ToString("0.00")}%");
		}
		ServiceContainer.Get<IOutput>().WriteLine($"Overall result = {totalHandledOverall}/{totalOverall} - {(100.0*totalHandledOverall/totalOverall).ToString("0.00")}%");
		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}


	public void Reset(GenerationState genState)
	{
	}
}