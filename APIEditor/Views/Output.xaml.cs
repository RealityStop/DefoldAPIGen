using System.Windows.Controls;
using APILib;
using APILib.Helpers;

namespace APIEditor.Views;

public partial class Output : UserControl
{
	public Output()
	{
		VM = ServiceContainer.Get<IOutput>();
		InitializeComponent();
		DataContext = VM;
	}


	public IOutput VM { get; set; }
}