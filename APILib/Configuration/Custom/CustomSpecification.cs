using System.Runtime.Serialization;
using APILib.Helpers;
using Newtonsoft.Json;

namespace APILib.Configuration;


[JsonConverter(typeof(JsonInheritanceConverter), "Type")]
[KnownType("GetKnownTypes")]
public abstract class CustomSpecification
{
	public virtual string Type { get; set; }
	public string Comment { get; set; } = "";
	public string Custom { get; set; }
	
	public static Type[] GetKnownTypes()
	{
		var types = typeof(CustomSpecification).GetInstantiableImplementors().ToArray();
		return types;
	}
}

public class CustomInterface : CustomSpecification
{
	public override string Type { get; set; } = nameof(CustomInterface);
	public string Contents { get; set; }
}

public class CustomEnum : CustomSpecification
{
	public override string Type { get; set; } = nameof(CustomEnum);
	
	public Dictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class CustomClass : CustomSpecification
{
	public override string Type { get; set; } = nameof(CustomClass);
	
	public bool IsStaticClass { get; set; }
	public static string DerivesFrom { get; set; }
	public bool IsSystemClass { get; set; }
	
	public List<CustomConstructor> Constructors { get; set; } = new List<CustomConstructor>();

	public List<CustomMethodInjector> Methods { get; set; } = new List<CustomMethodInjector>();
	public string Contents { get; set; }
}