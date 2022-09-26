namespace APILib.Analyzers;


public interface IAnalyzer
{
	AnalyzerResult Result { get; }

	bool RequirementsMet(GenerationState genState);

	void Execute(GenerationState genState);
	
	void Reset(GenerationState genState);
}