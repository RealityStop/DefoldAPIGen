using APILib.Analyzers.Artifacts;
using APILib.Helpers;

namespace APILib.Analyzers.Impls;

public class EnumerateAllLuaTypes :IAnalyzer
{
	public AnalyzerResult Result { get; set; }
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().Any()
				&& genState.Analyzers.Artifacts.Of<TrimmedAPIArtifact>().Any();
	}


	public void Execute(GenerationState genState)
	{
		var customHandlers = genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().First();
		
		
		HashSet<string> typeNames = new HashSet<string>();


		Result = new AnalyzerResult(AnalyzerResultType.Success);
		
		foreach (var rawAPI in genState.Analyzers.Artifacts.Of<TrimmedAPIArtifact>())
		{
			foreach (var nameSpace in rawAPI.Namespaces())
			{
				var functions = nameSpace.Elements.Where(x => x.Type.Equals("Function", StringComparison.OrdinalIgnoreCase));
				foreach (var function in functions)
				{
					foreach (var parameter in function.Parameters)
					{
						foreach (var allType in parameter.AllTypes())
						{
							if (string.IsNullOrEmpty(allType))
								continue;
							if (!typeNames.Contains(allType))
								typeNames.Add(allType);
						}
					}


					foreach (var returnValue in function.ReturnValues)
					{
						foreach (var allType in returnValue.AllTypes())
						{
							if (string.IsNullOrEmpty(allType))
								continue;
							if (!typeNames.Contains(allType))
								typeNames.Add(allType);
						}
					}
				}
			}
		}
		
		ServiceContainer.Get<IOutput>().WriteLine($"Discovered {typeNames.Count} types in the lua API");
		genState.Analyzers.Artifacts.AddArtifact(new AllLuaTypeNamesArtifact(typeNames));
	}


	public void Reset(GenerationState genState)
	{
	}
}