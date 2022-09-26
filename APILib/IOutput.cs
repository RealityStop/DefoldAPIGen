namespace APILib;

public interface IOutput
{
	void WriteLine(string message);
	void ReportException(Exception e);

	IDisposable NestTab();

	void OpenOutputFile();
}