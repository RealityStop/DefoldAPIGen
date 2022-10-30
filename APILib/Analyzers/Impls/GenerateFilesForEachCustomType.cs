using APILib.Analyzers.Artifacts;
using APILib.Analyzers.Generators;
using APILib.Configuration.CustomTypes;

namespace APILib.Analyzers.Impls;

public class GenerateFilesForEachCustomType : IAnalyzer
{
	private string _outputFolder;
	public AnalyzerResult Result { get; set; }
	public bool RequirementsMet(GenerationState genState)
	{
		return genState.Analyzers.Artifacts.Of<CustomTypesArtifact>().Any();
	}


	public void Execute(GenerationState genState)
	{
		var customTypesArtifacts = genState.Analyzers.Artifacts.Of<CustomTypesArtifact>();

		foreach (var typesArtifact in customTypesArtifacts)
		{
			foreach (var customType in typesArtifact.CustomTypes)
			{
				if (customType == CustomTypeDefinition.Void)
					continue;
				
				string output = "";
				if (customType.Specification is CustomClass customClass)
				{
					if (customClass.IsSystemClass)
						continue;
					output =  CustomClassRenderer.Render(customType);	
				}
				else if (customType.Specification is CustomEnum customEnum)
				{
					output = CustomEnumRenderer.Render(customType);
				}
				else if (customType.Specification is CustomInterface customInterface)
				{
					output = CustomInterfaceRenderer.Render(customType);
				}

				if (!string.IsNullOrEmpty(output))
				{
					string outputFilename = customType.Name;
					if (!string.IsNullOrWhiteSpace(customType.Filename))
						outputFilename = customType.Filename;
					
					var outputFile = Path.Combine(_outputFolder, $"{outputFilename}.cs");
					File.WriteAllText(outputFile, output);
				}
			}
		}

		Result = new AnalyzerResult(AnalyzerResultType.Success);
	}


	public void Reset(GenerationState genState)
	{
		_outputFolder = Path.Combine(genState.Settings.OutputLocation, "types");
	}
}