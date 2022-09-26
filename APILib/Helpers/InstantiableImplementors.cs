using System.Reflection;

namespace APILib.Helpers;

public static class InstantiableImplementors
{
	/// <summary>
	///     Returns all types in <paramref name="assembliesToSearch" /> that directly or indirectly implement or inherit from
	///     the given type.
	/// </summary>
	public static IEnumerable<Type> GetImplementors(this Type abstractType, params Assembly[] assembliesToSearch)
	{
		var typesInAssemblies = assembliesToSearch.SelectMany(assembly => assembly.GetTypes());
		return typesInAssemblies.Where(abstractType.IsAssignableFrom);
	}
	
	/// <summary>
	///     Determines whether <paramref name="type" /> is a concrete, non-open-generic type.
	/// </summary>
	public static bool IsInstantiable(this Type type)
	{
		return !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface);
	}
	
	/// <summary>
	///     Returns the results of <see cref="GetImplementors" /> that match <see cref="IsInstantiable" />.
	/// </summary>
	public static IEnumerable<Type> GetInstantiableImplementors(this Type abstractType)
	{
		var implementors = abstractType.GetImplementors(AppDomain.CurrentDomain.GetAssemblies());
		return implementors.Where(x => x.IsInstantiable());
	}
}