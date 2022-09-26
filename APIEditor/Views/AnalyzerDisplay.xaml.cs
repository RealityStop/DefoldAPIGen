using System.Windows.Controls;
using APIEditor.ViewModels;

namespace APIEditor.Views;

public partial class AnalyzerDisplay : UserControl
{
	public AnalyzerDisplay()
	{
		InitializeComponent();

		VM = new AnalyzerVM();
		DataContext = VM;
	}


	public AnalyzerVM VM { get; set; }
}