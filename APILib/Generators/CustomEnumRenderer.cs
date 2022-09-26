using APILib.Configuration;
using APILib.Helpers;

namespace APILib.Generators;

public static class CustomEnumRenderer
{
	public static string Render(CustomType customClass)
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
		builder.AppendLine("using defold.types;");
		builder.AppendLine("");
		builder.AppendLine("namespace defold.types");

		return builder.Scope();
	}
	
	

	private static IDisposable WriteClassSpecification(FormattedStringBuilder builder, CustomType customType)
	{
		var customClass = customType.Specification as CustomEnum;
		
		
		builder.AppendLine("/// <summary>");
		if (!string.IsNullOrWhiteSpace(customClass.Comment))
		{
			builder.AppendLine($"/// {customClass.Comment}");
			builder.AppendLine("/// ");
		}
		if (customType.Handling.Handling == CustomLuaHandling.HandlingType.Ignore)
			builder.AppendLine("/// @CSharpLua.Ignore");
		else if (customType.Handling.Handling == CustomLuaHandling.HandlingType.Template)
			builder.AppendLine($"/// @CSharpLua.Template = {customType.Handling.Template}");
		
		builder.AppendLine("/// </summary>");
		builder.AppendLine($"public enum {customType.Name}");
		
		return builder.Scope();
	}



	private static void WriteContent(FormattedStringBuilder builder, CustomType customType)
	{
		var customEnum = customType.Specification as CustomEnum;

		foreach (var value in customEnum.Values)
		{
			builder.AppendLine($"{value.Key} = {value.Value},");	
		}
	}
}