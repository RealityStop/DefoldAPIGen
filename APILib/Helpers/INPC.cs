using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APILib.Helpers;

public interface INotifyPropertyChangedExtended : INotifyPropertyChanged
{
	Action<string> RaisePropertyChanged { get; }
}

/// <summary>
///     A convenience wrapper for INotifyPropertyChangedExtended.  For constraints, use the full name.
/// </summary>
public interface INPCExtended : INotifyPropertyChangedExtended
{
}

public static class NotifyPropertyChangedExtension
{
	public static bool MutateVerbose<TField>(this INotifyPropertyChanged instance, ref TField field,
		TField newValue, Action<PropertyChangedEventArgs> raise, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<TField>.Default.Equals(field, newValue))
			return false;
		field = newValue;
		if (propertyName != null)
			raise?.Invoke(new PropertyChangedEventArgs(propertyName));
		return true;
	}


	public static bool MutateVerbose<TField>(this INotifyPropertyChanged instance, ref TField field,
		TField newValue, Action<string> raise, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<TField>.Default.Equals(field, newValue))
			return false;
		field = newValue;
		if (propertyName != null)
			raise?.Invoke(propertyName);
		return true;
	}
}