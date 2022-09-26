using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace APIEditor.Converters;

public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
	where T : ConverterMarkupExtension<T>, new()
{
	private static readonly object lockobject = new();
	private static T _converter;

	public static T Converter
	{
		get
		{
			//Shortcut lock if the converter is already made.  It will never be garbage collected.
			if (_converter != null)
				return _converter;
			lock (lockobject)
			{
				if (_converter == null)
					_converter = new T();
				return _converter;
			}
		}
	}

	public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
	public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);


	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return Converter;
	}
}