using APILib.Configuration.CustomTypes;
using APILib.Helpers;

namespace APILib.Analyzers.Generators;

public static class CustomClassRenderer
{
	public static string Render(CustomTypeDefinition customClass)
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
		builder.AppendLine("");
		builder.AppendLine("namespace types");

		return builder.Scope();
	}
	
	

	private static IDisposable WriteClassSpecification(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		var customClass = customType.Specification as CustomClass;
		
		
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
		if (!string.IsNullOrWhiteSpace(customClass.Custom))
			builder.AppendLine(customClass.Custom);
		else
			builder.AppendLine($"public {(customClass.IsStaticClass ? "static " : "")}class {customType.Name}");
		
		return builder.Scope();
	}
	
	

	private static void WriteContent(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		
		var customClass = customType.Specification as CustomClass;
		if (string.IsNullOrEmpty(customClass.Contents))
			return;

		var lines = customClass.Contents.Split(new char[] { '\n' });

		foreach (var line in lines)
		{
			builder.AppendLine(line);
		}
	}


	private static void WriteUserSpecifiedContent(FormattedStringBuilder builder, CustomClass customClass)
	{
		builder.AppendLine(customClass.Contents);
	}
}