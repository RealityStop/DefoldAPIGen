using APILib.Configuration.CustomTypes;
using APILib.Helpers;

namespace APILib.Analyzers.Generators;

public static class CustomEnumRenderer
{
	public static string Render(CustomTypeDefinition customClass)
	{
		if (!customClass.Specification.Type.Equals("CustomEnum"))
			throw new InvalidOperationException();
		
		
		FormattedStringBuilder builder = new FormattedStringBuilder();
		using (WriteInitialPreamble(builder))
		{
			using (WriteClassSpecification(builder, customClass))
			{
				WriteContent(builder, customClass);
			}
		}

		return builder.RenderToString();
	}


	private static IDisposable WriteInitialPreamble(FormattedStringBuilder builder)
	{
		builder.AppendLine("using System;");
		builder.AppendLine("using lua;");
		builder.AppendLine("");
		builder.AppendLine("namespace types");

		return builder.Scope();
	}
	
	

	private static IDisposable WriteClassSpecification(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		var customClass = customType.Specification as CustomEnum;
		
		
		builder.AppendLine("/// <summary>");
		if (!string.IsNullOrWhiteSpace(customClass.Comment))
		{
			builder.AppendLine($"/// {customClass.Comment}");
			builder.AppendLine("/// ");
		}

		if (customType.LuaHandling != null)
		{
			if (customType.LuaHandling.Handling == CustomLuaHandling.HandlingType.Ignore)
				builder.AppendLine("/// @CSharpLua.Ignore");
			else if (customType.LuaHandling.Handling == CustomLuaHandling.HandlingType.Template)
				builder.AppendLine($"/// @CSharpLua.Template = {customType.LuaHandling.Template}");
		}

		builder.AppendLine("/// </summary>");
		builder.AppendLine($"public enum {customType.Name}");
		
		return builder.Scope();
	}



	private static void WriteContent(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		var customEnum = customType.Specification as CustomEnum;

		foreach (var value in customEnum.Values)
		{
			builder.AppendLine($"{value.Key} = {value.Value},");	
		}
	}
}