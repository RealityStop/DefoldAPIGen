using APILib.Configuration.API;

namespace APILib.Configuration.Handlers;

public class HandlerOverrides
{
	public Dictionary<string, List<HandlerOverrideMethod>> Methods { get; set; }


	public bool TryFetchOverrideForFunction(DocElement function, out HandlerOverrideMethod outValue)
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