﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RegistryValley.App.Converters
{
    class StringToSurroundedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string strParam)
            {
                if (strParam == "quotes")
                {
                    return @$"""{value}""";
                }
                else if (strParam == "brackets")
                {
                    return $"({value})";
                }
                else if (strParam.Length == 2)
                {
                    return $"{strParam.First()}{value}{strParam.Last()}";
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
