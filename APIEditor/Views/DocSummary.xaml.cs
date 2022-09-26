using System.Windows.Controls;
using APIEditor.ViewModels;

namespace APIEditor.Views;

public partial class DocSummary : UserControl
{
	public DocSummary()
	{
		InitializeComponent();

		VM = new DocSummaryVM();
		DataContext = VM;
	}


	public DocSummaryVM VM { get; set; }
}