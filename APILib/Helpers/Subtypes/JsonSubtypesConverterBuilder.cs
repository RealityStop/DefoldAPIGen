﻿using Newtonsoft.Json;

namespace APILib.Helpers.Subtypes;
//  MIT License
//  
//  Copyright (c) 2017 Emmanuel Counasse
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

public class JsonSubtypesConverterBuilder
{
	private readonly NullableDictionary<object, Type> _subTypeMapping = new();
	private bool _addDiscriminatorFirst;
	private Type _baseType;
	private string _discriminatorProperty;
	private Type _fallbackSubtype;
	private bool _serializeDiscriminatorProperty;


	public static JsonSubtypesConverterBuilder Of(Type baseType, string discriminatorProperty)
	{
		var customConverterBuilder = new JsonSubtypesConverterBuilder
		{
			_baseType = baseType,
			_discriminatorProperty = discriminatorProperty
		};
		return customConverterBuilder;
	}


	public static JsonSubtypesConverterBuilder Of<T>(string discriminatorProperty)
	{
		return Of(typeof(T), discriminatorProperty);
	}


	public JsonSubtypesConverterBuilder SerializeDiscriminatorProperty()
	{
		return SerializeDiscriminatorProperty(false);
	}


	public JsonSubtypesConverterBuilder SerializeDiscriminatorProperty(bool addDiscriminatorFirst)
	{
		_serializeDiscriminatorProperty = true;
		_addDiscriminatorFirst = addDiscriminatorFirst;
		return this;
	}


	public JsonSubtypesConverterBuilder RegisterSubtype(Type subtype, object value)
	{
		_subTypeMapping.Add(value, subtype);
		return this;
	}


	public JsonSubtypesConverterBuilder RegisterSubtype<T>(object value)
	{
		return RegisterSubtype(typeof(T), value);
	}


	public JsonSubtypesConverterBuilder SetFallbackSubtype(Type fallbackSubtype)
	{
		_fallbackSubtype = fallbackSubtype;
		return this;
	}


	public JsonSubtypesConverterBuilder SetFallbackSubtype<T>(object value)
	{
		return RegisterSubtype(typeof(T), value);
	}


	public JsonConverter Build()
	{
		return new JsonSubtypesByDiscriminatorValueConverter(_baseType, _discriminatorProperty, _subTypeMapping,
			_serializeDiscriminatorProperty, _addDiscriminatorFirst, _fallbackSubtype);
	}
}