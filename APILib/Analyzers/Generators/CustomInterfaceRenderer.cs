using APILib.Configuration.CustomTypes;
using APILib.Helpers;

namespace APILib.Analyzers.Generators;

public static class CustomInterfaceRenderer
{
	public static string Render(CustomTypeDefinition customInterface)
	{
		if (!customInterface.Specification.Type.Equals("CustomInterface"))
			throw new InvalidOperationException();
		
		
		FormattedStringBuilder builder = new FormattedStringBuilder();
		using (WriteInitialPreamble(builder))
		{
			using (WriteInterfaceSpecification(builder, customInterface))
			{
				WriteContent(builder, customInterface);
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
	
	

	private static IDisposable WriteInterfaceSpecification(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		var customClass = customType.Specification as CustomInterface;
		
		
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
			else if (customType.LuaHandling?.Handling == CustomLuaHandling.HandlingType.Template)
				builder.AppendLine($"/// @CSharpLua.Template = {customType.LuaHandling.Template}");
		}

		builder.AppendLine("/// </summary>");
		if (!string.IsNullOrWhiteSpace(customClass.Custom))
			builder.AppendLine(customClass.Custom);
		else
			builder.AppendLine($"public interface {customType.Name}");
		
		return builder.Scope();
	}
	
	

	private static void WriteContent(FormattedStringBuilder builder, CustomTypeDefinition customType)
	{
		var customInterface = customType.Specification as CustomInterface;

		if (!string.IsNullOrWhiteSpace(customInterface.Contents))
		{
			WriteUserSpecifiedContent(builder, customInterface);
			return;
		}
	}


	private static void WriteUserSpecifiedContent(FormattedStringBuilder builder, CustomInterface customInterface)
	{
		builder.AppendLine(customInterface.Contents);
	}
}