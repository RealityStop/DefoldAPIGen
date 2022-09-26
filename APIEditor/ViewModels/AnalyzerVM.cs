using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using APILib;
using APILib.Analyzers;
using APILib.Helpers;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace APIEditor.ViewModels;

public class AnalyzerVM : INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
	
	
	private bool _allAnalyzersPassing = default;
	public bool AllAnalyzersPassing { get =>  _allAnalyzersPassing; set => this.MutateVerbose(ref _allAnalyzersPassing, value, RaisePropertyChanged); }

	public GenerationState State { get; }

	public ObservableCollectionExtended<IAnalyzer> Analyzers { get; } = new ObservableCollectionExtended<IAnalyzer>();

	
	private IAnalyzer _selectedItem = default;
	public IAnalyzer SelectedItem { get =>  _selectedItem; set => this.MutateVerbose(ref _selectedItem, value, RaisePropertyChanged); }
	
	public ReactiveCommand<Unit, Task> DataGridDoubleClick { get; }


	public AnalyzerVM()
	{
		State = StateLocator.CurrentState;

		State.Analyzers.Analyzers.Connect().Bind(Analyzers).Subscribe();

		async void OnNext((string?, string?) x)
		{
			Task.Run(async () =>
			{
				State.Analyzers.InitializeTo(State);
				await State.Analyzers.AnalyzeAsync();
			});
		}

		this.WhenAnyValue(x => x.State.Settings.APILocation, x => x.State.Settings.HandlersLocation)
			.Where(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2))
			.Subscribe(OnNext);
		
		DataGridDoubleClick = ReactiveCommand.Create<Unit, Task>(async (_) =>
		{
			
			
		});
		
	}
}