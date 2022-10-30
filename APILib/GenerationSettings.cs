using System.ComponentModel;
using System.Reactive.Disposables;
using APILib.Helpers;
using Newtonsoft.Json;
using ReactiveUI;

namespace APILib;

[JsonObject(MemberSerialization.OptIn)]
public class GenerationSettings : INotifyPropertyChangedExtended
{
	public event PropertyChangedEventHandler? PropertyChanged;
	public Action<PropertyChangedEventArgs> InvokePropertyChanged => args => PropertyChanged?.Invoke(this, args);
	public Action<string> RaisePropertyChanged => arg => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
	
	
	private string? _apiLocation;
	[JsonProperty]
	public string? APILocation { get =>  _apiLocation; set => this.MutateVerbose(ref _apiLocation, value, RaisePropertyChanged); }

	private string? _handlersLocation;
	[JsonProperty]
	public string? HandlersLocation { get =>  _handlersLocation; set => this.MutateVerbose(ref _handlersLocation, value, RaisePropertyChanged); }

	private string? _outputLocation = ".";
	[JsonProperty]
	public string? OutputLocation { get =>  _outputLocation; set => this.MutateVerbose(ref _outputLocation, value, RaisePropertyChanged); }
	
	
	private bool _isValid;
	public bool IsValid { get =>  _isValid; set => this.MutateVerbose(ref _isValid, value, RaisePropertyChanged); }


	private CompositeDisposable _disposable = new CompositeDisposable();
	
	
	public GenerationSettings()
	{
		LoadFromSettings();

		
		this.WhenAnyValue(x => x.APILocation, x=>x.HandlersLocation, (x,y)=>!string.IsNullOrWhiteSpace(x) && !string.IsNullOrWhiteSpace(y) )
			.Subscribe(x=>IsValid = x)
			.DisposeWith(_disposable);

		
		PropertyChanged += SaveSettings;
	}


	private void SaveSettings(object? sender, PropertyChangedEventArgs e)
	{
		if (SettingsLocation == null)
		{
			Console.WriteLine("Error!  No settings location");
			return;
		}
		
		File.WriteAllText(SettingsLocation, JsonConvert.SerializeObject(this));
	}


	private void LoadFromSettings()
	{
		if (File.Exists(SettingsLocation))
		{
			var settings= File.ReadAllText(SettingsLocation);
			JsonConvert.PopulateObject(settings, this);
		}
	}


	public string? SettingsLocation => "generationSettings.config";
}