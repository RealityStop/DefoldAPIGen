using APILib.API;

namespace APILib.Configuration.Handlers;

public class Overrides
{
	public Dictionary<string, List<OverrideMethod>> Methods { get; set; }


	public bool TryFetchOverrideForFunction(DocElement function, out OverrideMethod outValue)
	{
		if (Methods.TryGetValue(function.FunctionName(), out var overrideList))
		{
			foreach (var overrideMethod in overrideList)
			{
				if (overrideMethod.TargetParameterCount != function.Parameters.Count)
					continue;
				if (overrideMethod.TargetReturnValueCount != function.ReturnValues.Count)
					continue;

				outValue = overrideMethod;
				return true;
			}
		}

		outValue = default;
		return false;
	}
}