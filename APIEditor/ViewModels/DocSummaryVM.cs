using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive.Linq;
using APILib;
using APILib.Analyzers.Artifacts;
using APILib.Configuration.API;
using APILib.Helpers;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace APIEditor.ViewModels;

public class DocSummaryVM : INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));

	public GenerationState State { get; }


	private SourceList<DocJson> _availableAPIs = new SourceList<DocJson>();
	public ObservableCollectionExtended<DocJson> AvailableAPIs { get; } = new ObservableCollectionExtended<DocJson>();


	public DocSummaryVM()
	{
		_availableAPIs.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(AvailableAPIs).Subscribe();
		
		
		State = StateLocator.CurrentState;
		State.Analyzers.Artifacts.WatchArtifactOf<TrimmedAPIArtifact>()
			.ObserveOnDispatcher()
			.OnItemAdded(x =>
			{
				_availableAPIs.Clear();
				foreach (var value in x.Namespaces())
				{
					_availableAPIs.Add(value);
				}
			})
			.Subscribe();
	}
}