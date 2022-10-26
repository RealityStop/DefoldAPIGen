using System.ComponentModel;
using System.Diagnostics;
using APILib.Analyzers.Artifacts;
using APILib.Configuration.API;
using APILib.Helpers;

namespace APILib.Analyzers.Impls;

/// <summary>
/// Analyzer responsible for loading Doc API and registering it in the artifacts.
/// </summary>
public class DocAPILoadAndParse : IAnalyzer, INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));

	
	private AnalyzerResult _result = default;
	public AnalyzerResult Result { get =>  _result; set => this.MutateVerbose(ref _result, value, RaisePropertyChanged); }
	

	public bool RequirementsMet(GenerationState genState)
	{
		if (String.IsNullOrEmpty(genState.Settings.APILocation))
			return false;
		
		return true;
	}


	public void Execute(GenerationState genState)
	{
		try
		{
			Debug.Assert(genState.Settings.APILocation != null, "generationSettings.APILocation != null");
		
			//Load and inject the API as an artifact, so other analyzers can use it.
			var loader = new FolderLoader(genState.Settings.APILocation);
			var rawAPIs = new RawAPIArtifact(loader.Load(new DocJsonLoader()).ToDictionary(x=>x.Info.Namespace, x=>x));
			genState.Analyzers.Artifacts.AddArtifact(rawAPIs);
			Result = new AnalyzerResult(AnalyzerResultType.Success);
			Result.AddMessage($"{rawAPIs.Namespaces().Count()} apis loaded");
			
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