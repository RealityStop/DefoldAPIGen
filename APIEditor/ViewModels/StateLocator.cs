using APILib;

namespace APIEditor.ViewModels;

public static class StateLocator
{
	public static GenerationState CurrentState { get; } = new GenerationState();
}