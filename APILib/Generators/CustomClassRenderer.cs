using APILib.Configuration;
using APILib.Helpers;

namespace APILib.Generators;

public static class CustomClassRenderer
{
	public static string Render(CustomType customClass)
	{
		if (!customClass.Specification.Type.Equals("CustomClass"))
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
		builder.AppendLine("using defold;");
		builder.AppendLine("");
		builder.AppendLine("namespace defold.types");

		return builder.Scope();
	}
	
	

	private static IDisposable WriteClassSpecification(FormattedStringBuilder builder, CustomType customType)
	{
		var customClass = customType.Specification as CustomClass;
		
		
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
		if (!string.IsNullOrWhiteSpace(customClass.Custom))
			builder.AppendLine(customClass.Custom);
		else
			builder.AppendLine($"public {(customClass.IsStaticClass ? "static " : "")}class {customType.Name}");
		
		return builder.Scope();
	}
	
	

	private static void WriteContent(FormattedStringBuilder builder, CustomType customType)
	{
		var customClass = customType.Specification as CustomClass;

		if (!string.IsNullOrWhiteSpace(customClass.Contents))
		{
			WriteUserSpecifiedContent(builder, customClass);
			return;
		}
	}


	private static void WriteUserSpecifiedContent(FormattedStringBuilder builder, CustomClass customClass)
	{
		builder.AppendLine(customClass.Contents);
	}
}