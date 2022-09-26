using System.Windows.Controls;
using APIEditor.ViewModels;

namespace APIEditor.Views;

public partial class GenerationControls : UserControl
{
	public GenerationControls()
	{
		VM = new GenerateVM();
		InitializeComponent();

		DataContext = VM;
	}


	public GenerateVM VM { get; set; }
}