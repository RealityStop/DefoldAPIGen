using System.Reactive.Disposables;
using System.Text;

namespace APILib.Helpers;

public class FormattedStringBuilder
{
	private StringBuilder _builder = new StringBuilder();

	private int _tabLevel = 0;
	private string _tabPrefix = "";



	public IDisposable Scope()
	{
		AppendLine("{");
		_tabLevel++;
		_tabPrefix += "\t";
		return Disposable.Create(Unscope);
	}


	private void Unscope()
	{
		if (_tabPrefix.Length >= 1)
			_tabPrefix = _tabPrefix.Substring(0, _tabPrefix.Length - 1);
		
		_tabLevel = Math.Max(0, _tabLevel - 1);
		AppendLine("}");
	}

	private string OffsetByTabs(string message)
	{
		if (string.IsNullOrEmpty(message))
			return message;
		
		return $"{_tabPrefix}{message}";
	}
	
	public void AppendLine(string s)
	{
		_builder.AppendLine(OffsetByTabs(s));
	}


	public string RenderToString()
	{
		return _builder.ToString();
	}
}