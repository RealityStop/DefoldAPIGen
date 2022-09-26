namespace APILib;

public interface IFileLoader<T>
{
	bool FilterFilename(string file);
	T Parse(string file);
	string APIType { get; }
}