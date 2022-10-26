using System.Windows;
using APILib.Configuration;
using APILib.Configuration.CustomTypes;

namespace APIEditor.Views;

public partial class HandlerEditor : Window
{
	public HandlerEditor(CustomTypeDefinition customClass)
	{
		InitializeComponent();
	}
}