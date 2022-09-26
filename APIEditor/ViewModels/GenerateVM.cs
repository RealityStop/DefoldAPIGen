using System;
using System.ComponentModel;
using System.Net;
using System.Reactive;
using APILib;
using APILib.Helpers;
using ReactiveUI;

namespace APIEditor.ViewModels;

public class GenerateVM : INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));


	public GenerationState State { get; }

	public ReactiveCommand<Unit, Unit> OpenLogCMD { get; }


	public GenerateVM()
	{
		State = StateLocator.CurrentState;


		OpenLogCMD = ReactiveCommand.Create(() =>
		{	
			ServiceContainer.Get<IOutput>().OpenOutputFile();
		});
	}
}