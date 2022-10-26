using APILib.Analyzers.Artifacts;
using APILib.Helpers;

namespace APILib.Analyzers.Impls;

public class FilterDocs : IAnalyzer
{
	public AnalyzerResult Result { get; set; }
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().Any() &&
		       genState.Analyzers.Artifacts.Of<RawAPIArtifact>().Any() &&
		       genState.Analyzers.Artifacts.Of<FixedAPIArtifact>().Any();
	}


	public void Execute(GenerationState genState)
	{
		Result = new AnalyzerResult(AnalyzerResultType.Success);
		
		var customHandlers = genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().Single();
		var rawAPI = genState.Analyzers.Artifacts.Of<RawAPIArtifact>().Single();

		var trimmedAPI = new TrimmedAPIArtifact(rawAPI.Namespaces().Where(nameSpace =>
		{
			if (customHandlers.Handling.TryGetValue(nameSpace.Info.Namespace, out var handling))
			{
				if (handling.Ignore)
					return false;

			}

			return true;
		}).ToDictionary(x => x.Info.Namespace, x => x));

		ServiceContainer.Get<IOutput>().WriteLine($"{trimmedAPI.Namespaces().Count()} APIs marked for generation");
		
		genState.Analyzers.Artifacts.AddArtifact(trimmedAPI);
	}


	public void Reset(GenerationState genState)
	{
	}
}