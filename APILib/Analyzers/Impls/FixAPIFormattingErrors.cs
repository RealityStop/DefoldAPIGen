using System.Text.RegularExpressions;
using APILib.API;
using APILib.Artifacts;

namespace APILib.Analyzers;

public class FixAPIFormattingErrors : IAnalyzer
{
	public AnalyzerResult Result { get; set; }


	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>().Any() &&
		       genState.Analyzers.Artifacts.Of<RawAPIArtifact>().Any();
	}


	private static Regex oddBracketMatcher = new Regex(@"^(\w+)]\s*(.*\[.*)$");


	public void Execute(GenerationState genState)
	{
		var customHandler = genState.Analyzers.Artifacts.Of<CustomHandlersArtifact>();
		var rawAPI = genState.Analyzers.Artifacts.Of<RawAPIArtifact>();

		foreach (var api in rawAPI)
		{
			foreach (var nameSpace in api.Namespaces())
			{
				foreach (var function in nameSpace.Functions())
				{
					FixParameters(function);
					FixReturnValues(function);
				}
			}
		}

		genState.Analyzers.Artifacts.AddArtifact(new FixedAPIArtifact());
		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}


	public void Reset(GenerationState genState)
	{

	}


	private static void FixParameters(DocElement function)
	{
		foreach (var functionParameter in function.Parameters)
		{
			if (!string.IsNullOrEmpty(functionParameter.type))
			{
				var match = oddBracketMatcher.Match(functionParameter.type);
				if (match.Success)
				{
					functionParameter.type = match.Groups[1].Value;
					functionParameter.SpecialTypeComments[functionParameter.type] = match.Groups[2].Value;
				}
			}

			if (functionParameter.types != null && functionParameter.types.Any())
			{
				for (int i = 0; i < functionParameter.types.Count; i++)
				{
					var match = oddBracketMatcher.Match(functionParameter.types[i]);
					if (match.Success)
					{
						functionParameter.types[i] = match.Groups[1].Value;
						functionParameter.SpecialTypeComments[functionParameter.types[i]] = match.Groups[2].Value;
					}
				}
			}
		}
	}


	private static void FixReturnValues(DocElement function)
	{
		foreach (var returnValue in function.ReturnValues)
		{
			if (!string.IsNullOrEmpty(returnValue.type))
			{
				var match = oddBracketMatcher.Match(returnValue.type);
				if (match.Success)
				{
					returnValue.type = match.Groups[1].Value;
					returnValue.SpecialTypeComments[returnValue.type] = match.Groups[2].Value;
				}
			}

			if (returnValue.types != null && returnValue.types.Any())
			{
				for (int i = 0; i < returnValue.types.Count; i++)
				{
					var match = oddBracketMatcher.Match(returnValue.types[i]);
					if (match.Success)
					{
						returnValue.types[i] = match.Groups[1].Value;
						returnValue.SpecialTypeComments[returnValue.types[i]] = match.Groups[2].Value;
					}
				}
			}
		}
	}
}