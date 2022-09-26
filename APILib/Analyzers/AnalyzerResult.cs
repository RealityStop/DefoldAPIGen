using APILib.Helpers;

namespace APILib.Analyzers;

public enum AnalyzerResultType {NotRun, Fail, Success}
public class AnalyzerResult
{
	private static IOutput Output { get; } = ServiceContainer.Get<IOutput>();
	
	public AnalyzerResultType Result { get; private set; }

	public List<string> Messages { get; } = new List<string>();


	public AnalyzerResult(AnalyzerResultType result, params string[] messages)
	{
		Result = result;

		foreach (var message in messages)
		{
			AddMessage(message);
		}
	}


	public void Fail(string message)
	{
		Result = AnalyzerResultType.Fail;
		AddMessage(message);
	}


	public void AddMessage(string message)
	{
		Messages.Add(message);
		Output.WriteLine(message);
	}


	public void Fail(Exception e)
	{
		AddMessage(e.Message);
		if (e.StackTrace != null)
			AddMessage(e.StackTrace);
	}
}