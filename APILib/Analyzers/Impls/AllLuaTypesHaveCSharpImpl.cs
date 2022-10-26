using System.ComponentModel;
using APILib.Analyzers.Artifacts;
using APILib.Helpers;

namespace APILib.Analyzers.Impls;

public class AllLuaTypesHaveCSharpImpl : IAnalyzer, INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
	
	private AnalyzerResult _result = default;
	public AnalyzerResult Result { get =>  _result; set => this.MutateVerbose(ref _result, value, RaisePropertyChanged); }


	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<CustomTypesArtifact>().Any() && genState.Analyzers.Artifacts.Of<AllLuaTypeNamesArtifact>().Any();
	}


	public void Execute(GenerationState genState)
	{
		var custom =  genState.Analyzers.Artifacts.Of<CustomTypesArtifact>().First();
		var rawAPInames =  genState.Analyzers.Artifacts.Of<AllLuaTypeNamesArtifact>().First();

		//We record a list so we only report each type once
		HashSet<string> typesNotFound = new HashSet<string>();

		Result = new AnalyzerResult(AnalyzerResultType.Success);

		foreach (var name in rawAPInames.TypeNames)
		{
			if (!custom.TryGetCustomImplFor(name, out _))
			{
				if (!typesNotFound.Contains(name))
				{
					Result.Fail(
						$"lua type `{name}` has no custom implementation.");
					typesNotFound.Add(name);
				}
			}
		}
	}


	public void Reset(GenerationState genState)
	{
		Result = new AnalyzerResult(AnalyzerResultType.NotRun);
	}
}