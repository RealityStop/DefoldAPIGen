using System.Windows.Controls;
using APIEditor.ViewModels;

namespace APIEditor.Views;

public partial class HandlerDisplay : UserControl
{
	public HandlerDisplay()
	{
		VM = new HandlerDisplayVM();
		InitializeComponent();
		DataContext = VM;
	}


	public HandlerDisplayVM VM { get; set; }
}