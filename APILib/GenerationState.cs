using System.Reactive.Linq;
using APILib.Analyzers;
using ReactiveUI;

namespace APILib;

public class GenerationState
{
	public GenerationSettings Settings { get; } = new GenerationSettings();
	public AnalyzerRoot Analyzers { get; } = new AnalyzerRoot();


	public GenerationState()
	{

	}
}