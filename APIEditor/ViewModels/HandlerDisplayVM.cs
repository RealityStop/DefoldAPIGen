using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using APIEditor.Views;
using APILib;
using APILib.Configuration;
using APILib.Helpers;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using MessageBox = HandyControl.Controls.MessageBox;

namespace APIEditor.ViewModels;

public class HandlerDisplayVM : INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));

	private CustomType? _selectedItem = default;
	public CustomType? SelectedItem { get =>  _selectedItem; set => this.MutateVerbose(ref _selectedItem, value, RaisePropertyChanged); }
	
	
	public GenerationState State { get; }

	public ObservableCollectionExtended<CustomType> CustomHandlers { get; } = new ObservableCollectionExtended<CustomType>();
	
	public ReactiveCommand<Unit, Unit> CreateNewCMD { get; }
	public ReactiveCommand<Unit, Task> DataGridDoubleClick { get; }
	
	public HandlerDisplayVM()
	{
		State = StateLocator.CurrentState;
		
		State.Analyzers.Artifacts.WatchArtifactOf<CustomTypesArtifact>()
			.ObserveOnDispatcher()
			.TransformMany(x=>x.CustomTypes)
			.Bind(CustomHandlers)
			.Subscribe();
		
		DataGridDoubleClick = ReactiveCommand.Create<Unit, Task>(async (_) =>
		{
			if (SelectedItem == null)
				return;

			var editor = new HandlerEditor(SelectedItem);
			editor.ShowDialog();
		});
		
		CreateNewCMD = ReactiveCommand.Create(() =>
		{
			if (!State.Settings.IsValid)
				MessageBox.Show("Please configure before adding handlers.");

			Debug.Assert(State.Settings.HandlersLocation != null, "State.Settings.HandlersLocation != null");


			if (!Directory.Exists(State.Settings.HandlersLocation))
				if (MessageBoxResult.Yes == MessageBox.Show("Handlers directory does not exist.  Create?",
					    "Create directory?", MessageBoxButton.YesNo))
					Directory.CreateDirectory(State.Settings.HandlersLocation);

			var newHandler = new CustomType() { Name = "New_Class" };
			var editor = new HandlerEditor(newHandler);
			editor.ShowDialog();

			CustomTypeInjectorLoader.Save(newHandler);
		});
		
		
	}
}