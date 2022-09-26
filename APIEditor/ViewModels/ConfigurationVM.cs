using System;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using APILib;
using APILib.Helpers;
using Ookii.Dialogs.Wpf;
using ReactiveUI;

namespace APIEditor.ViewModels;

public class ConfigurationVM : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
	
	
	private readonly ReactiveCommand<Unit, Unit>? _browseForAPICMD;
	public ReactiveCommand<Unit, Unit>? BrowseForAPICMD { get =>  _browseForAPICMD;
		private init => this.MutateVerbose(ref _browseForAPICMD, value, RaisePropertyChanged); }

	private readonly ReactiveCommand<Unit, Unit>? _browseForHandlersCMD;
	public ReactiveCommand<Unit, Unit>? BrowseForHandlersCMD { get =>  _browseForHandlersCMD;
		private init => this.MutateVerbose(ref _browseForHandlersCMD, value, RaisePropertyChanged); }
	
	
	private bool _hasSettingsAssigned;
	public bool HasSettingsAssigned { get =>  _hasSettingsAssigned; set => this.MutateVerbose(ref _hasSettingsAssigned, value, RaisePropertyChanged); }
	
	
	private string? _apiVersion;
	public string? APIVersion { get =>  _apiVersion; set => this.MutateVerbose(ref _apiVersion, value, RaisePropertyChanged); }

	public GenerationState State { get; }
	
	private readonly CompositeDisposable _disposings = new CompositeDisposable();

	public ConfigurationVM()
	{
		State = StateLocator.CurrentState;
		BrowseForAPICMD = ReactiveCommand.Create(() =>
		{
			VistaFolderBrowserDialog browserDialog = new VistaFolderBrowserDialog();
			browserDialog.ShowNewFolderButton = false;
			browserDialog.Multiselect = false;
			if (Directory.Exists(State.Settings!.APILocation))
				browserDialog.SelectedPath = State.Settings.APILocation;
			var result = browserDialog.ShowDialog();
			if (result.HasValue && result.Value)
			{
				State.Settings.APILocation = browserDialog.SelectedPath;
			}
		}, this.WhenAnyValue(x=>x.HasSettingsAssigned))
			.DisposeWith(_disposings);
		
		BrowseForHandlersCMD = ReactiveCommand.Create(() =>
		{
			VistaFolderBrowserDialog browserDialog = new VistaFolderBrowserDialog();
			browserDialog.ShowNewFolderButton = false;
			browserDialog.Multiselect = false;
			if (Directory.Exists(State.Settings!.HandlersLocation))
				browserDialog.SelectedPath = State.Settings.HandlersLocation;
			var result = browserDialog.ShowDialog();
			if (result.HasValue && result.Value)
			{
				State.Settings.HandlersLocation = browserDialog.SelectedPath;
			}
		}, this.WhenAnyValue(x=>x.HasSettingsAssigned))
			.DisposeWith(_disposings);


		this.WhenAnyValue(x => x.State.Settings!.APILocation).WhereNotNull().Subscribe(location =>
		{
			AttemptExtractVersionNumber(location);
			ScanDocs(location);
		}).DisposeWith(_disposings);
	}


	private void ScanDocs(string location)
	{
	}


	private void AttemptExtractVersionNumber(string x)
	{
		Regex versionMatcher = new Regex(@"^((\d+)\.(\d+)\.(\d+))$");

		var directoryName = Path.GetFileName(Path.GetDirectoryName(x));
		if (directoryName == null)
		{
			APIVersion = "Unknown Version";
			return;
		}

		var match = versionMatcher.Match(directoryName);

		if (match.Success)
			APIVersion = match.Groups[1].Value;
		else
			APIVersion = "Unknown Version";
	}
}