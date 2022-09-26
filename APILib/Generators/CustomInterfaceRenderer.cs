using APILib.Configuration;
using APILib.Helpers;

namespace APILib.Generators;

public static class CustomInterfaceRenderer
{
	public static string Render(CustomType customInterface)
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
		builder.AppendLine("using defold;");
		builder.AppendLine("");
		builder.AppendLine("namespace defold.types");

		return builder.Scope();
	}
	
	

	private static IDisposable WriteInterfaceSpecification(FormattedStringBuilder builder, CustomType customType)
	{
		var customClass = customType.Specification as CustomInterface;
		
		
		builder.AppendLine("/// <summary>");
		if (!string.IsNullOrWhiteSpace(customClass.Comment))
		{
			builder.AppendLine($"/// {customClass.Comment}");
			builder.AppendLine("/// ");
		}
		if (customType.Handling == null)
			builder.AppendLine("/// @CSharpLua.Ignore");
		else if (customType.Handling.Handling == CustomLuaHandling.HandlingType.Ignore)
			builder.AppendLine("/// @CSharpLua.Ignore");
		else if (customType.Handling?.Handling == CustomLuaHandling.HandlingType.Template)
			builder.AppendLine($"/// @CSharpLua.Template = {customType.Handling.Template}");
		
		builder.AppendLine("/// </summary>");
		if (!string.IsNullOrWhiteSpace(customClass.Custom))
			builder.AppendLine(customClass.Custom);
		else
			builder.AppendLine($"public interface {customType.Name}");
		
		return builder.Scope();
	}
	
	

	private static void WriteContent(FormattedStringBuilder builder, CustomType customType)
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