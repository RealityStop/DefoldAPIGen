using System.Reactive.Linq;
using System.Reactive.Subjects;
using APILib.Analyzers;
using DynamicData;

namespace APILib;

public class AnalyzerArtifacts
{
	private SourceList<IArtifact> _allArtifacts = new SourceList<IArtifact>();

	private Subject<IArtifact> _artifactModified = new Subject<IArtifact>();
	public IObservable<IArtifact> ArtifactModified  => _artifactModified;


	public IObservable<IChangeSet<T>> WatchArtifactOf<T>() where T : IArtifact
	{
		return _allArtifacts.Connect().Filter(x => x is T).Transform(x=>(T)x);
	}

	

	public void AddArtifact(IArtifact artifact)
	{
		_allArtifacts.Add(artifact);
		_artifactModified.OnNext(artifact);
	}


	public void Clear()
	{
		_allArtifacts.Clear();
	}


	public IEnumerable<T> Of<T>() where T : IArtifact
	{
		return _allArtifacts.Items.OfType<T>();
	}
}