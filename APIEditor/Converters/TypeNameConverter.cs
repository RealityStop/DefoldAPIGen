using System;
using System.Globalization;
using System.Windows.Data;

namespace APIEditor.Converters;

public class TypeNameConverter : ConverterMarkupExtension<TypeNameConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is null)
			return Binding.DoNothing;

		return value.GetType().Name;
	}


	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}