using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using APILib.Helpers;
using DynamicData;

namespace APILib.Analyzers;

public class AnalyzerRoot : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
	
	private bool _isAnalyzing = default;
	public bool IsAnalyzing { get =>  _isAnalyzing; set => this.MutateVerbose(ref _isAnalyzing, value, RaisePropertyChanged); }

	public AnalyzerArtifacts Artifacts { get; } = new AnalyzerArtifacts();
	
	
	private GenerationState? _state;
	private readonly IOutput _output;

	private bool _analyzersSucceeded = default;
	public bool AnalyzersSucceeded { get =>  _analyzersSucceeded; set => this.MutateVerbose(ref _analyzersSucceeded, value, RaisePropertyChanged); }
	
	public SourceCache<IAnalyzer, string> Analyzers { get; } =
		new SourceCache<IAnalyzer, string>(x => x.GetType().Name);


	public AnalyzerRoot()
	{
		_output = ServiceContainer.Get<IOutput>();
		
		var analyzers  = Assembly.GetExecutingAssembly().GetTypes()
			.Where(t => typeof(IAnalyzer).IsAssignableFrom(t) && t.IsClass).Select(x=>(IAnalyzer)Activator.CreateInstance(x));
		
		Analyzers.Edit(updater =>
		{
			foreach (var analyzer in analyzers)
			{
				if (analyzer != null)
				{
					_output.WriteLine($"Found analyzer: {analyzer.GetType().Name}");
					updater.AddOrUpdate(analyzer);
				}
			}
		});
		
	}


	public void InitializeTo(GenerationState state)
	{
		_state =state;
	}


	public async Task<bool> AnalyzeAsync()
	{
		Debug.Assert(_state != null, nameof(_state) + " != null");

		_output.WriteLine("Analyzers starting...");
		using (_output.NestTab())
		{
			IsAnalyzing = true;

			Reset();

			int iterationCount = 0;
			int iterationMax = 100;
			bool atLeastOneExecuted = false;
			bool allSuccess = true;

			var remainingAnalyzers = Analyzers.Items.ToList();
			List<IAnalyzer> analyzersCompletedThisCycle = new List<IAnalyzer>();

			do
			{
				atLeastOneExecuted = false;

				//Execute all analyzers that are ready to analyze
				foreach (var analyzer in remainingAnalyzers)
				{
					if (analyzer.RequirementsMet(_state))
					{
						_output.WriteLine($"{analyzer.GetType().Name} is ready to run.  Start...");
						var start =DateTime.Now;

						using (_output.NestTab())
						{
							try
							{

								analyzer.Execute(_state);
								if (analyzer.Result == null)
								{
									_output.WriteLine($"{analyzer.GetType().Name} FAILED due to no result.");
									allSuccess = false;
								}

								if (analyzer.Result.Result == AnalyzerResultType.Fail)
								{
									_output.WriteLine($"{analyzer.GetType().Name} FAILED.");
									allSuccess = false;
								}

							}
							catch (Exception e)
							{
								_output.ReportException(e);
								throw;
							}
						}

						var end = DateTime.Now;
						_output.WriteLine($"{analyzer.GetType().Name} is finished ({analyzer.Result.Result.ToString()}).  ({(end - start).TotalSeconds.ToString("0.00")}s elapsed)");

						analyzersCompletedThisCycle.Add(analyzer);
						atLeastOneExecuted = true;
					}
				}

				//Now we can remove the ones that completed
				foreach (var analyzer in analyzersCompletedThisCycle)
				{
					remainingAnalyzers.Remove(analyzer);
				}

				analyzersCompletedThisCycle.Clear();
				iterationCount++;

			} while (atLeastOneExecuted && remainingAnalyzers.Count > 0 && iterationCount < iterationMax);

			
			if (remainingAnalyzers.Count() != 0)
			{
				//Uh oh... things didn't resolve.
				
				
				_output.WriteLine($"One or more analyzers never resolved dependencies.  Execution aborted.");
				using (_output.NestTab())
				{
					foreach (var analyzer in remainingAnalyzers)
					{
						_output.WriteLine($"{analyzer.GetType().Name} failed to resolve");
					}
				}

				AnalyzersSucceeded = false;
			}
			else
				AnalyzersSucceeded = allSuccess;
			

			IsAnalyzing = false;
		}

		return true;
	}


	private void Reset()
	{
		foreach (var item in Analyzers.Items)
		{
			item.Reset(_state);
		}
		Artifacts.Clear();
	}
}