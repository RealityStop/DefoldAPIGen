using System.Windows.Controls;
using APIEditor.ViewModels;

namespace APIEditor.Views;

public partial class Configuration : UserControl
{
	public Configuration()
	{
		VM = new ConfigurationVM();
		InitializeComponent();
		DataContext = VM;
	}


	public ConfigurationVM VM { get; }
}