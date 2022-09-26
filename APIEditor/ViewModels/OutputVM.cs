using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using APILib;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace APIEditor.ViewModels;

public class OutputVM : IOutput
{
	const string outputFile = "Output.txt";
	private StreamWriter _writer = new StreamWriter(outputFile, new FileStreamOptions(){Access = FileAccess.Write, Share = FileShare.ReadWrite, Mode = FileMode.Create});
	private SourceList<string> _outputLines = new SourceList<string>();
	public ObservableCollectionExtended<string> OutputLines { get; } = new ObservableCollectionExtended<string>();


	private int _tabLevel = 0;
	private string _tabPrefix = "";


	public OutputVM()
	{
		_outputLines.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(OutputLines).Subscribe();
	}


	public void WriteLine(string message)
	{
		message = OffsetByTabs(message);
		_writer.WriteLine(message);
		_outputLines.Add(message);
		_writer.Flush();
	}


	public void ReportException(Exception e)
	{
		WriteLine("Exception!!!");
		WriteLine(e.Message);
		if (e.StackTrace != null)
			WriteLine(e.StackTrace);
	}


	private string OffsetByTabs(string message)
	{
		return $"{_tabPrefix}{message}";
	}


	public IDisposable NestTab()
	{
		_tabLevel++;
		_tabPrefix += "\t";
		return Disposable.Create(UnnestTabs);
	}


	public void UnnestTabs()
	{
		if (_tabPrefix.Length >= 1)
			_tabPrefix = _tabPrefix.Substring(0, _tabPrefix.Length - 1);
		
		_tabLevel = Math.Max(0, _tabLevel - 1);
	}


	public void OpenOutputFile()
	{
		Process.Start("explorer.exe", outputFile);
	}
}