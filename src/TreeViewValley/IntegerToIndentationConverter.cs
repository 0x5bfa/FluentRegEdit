using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.Foundation.Metadata;

namespace TreeViewValley
{
    [WebHostHidden]
    public sealed class IntegerToIndentationConverter : IValueConverter
    {
        private int indentMultiplier;
        public int IndentMultiplier { get => indentMultiplier; set => indentMultiplier = value; }

        IntegerToIndentationConverter()
        {
            //default
            indentMultiplier = 20;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Thickness indent = new(0);

            if (value != null)
            {
                indent.Left = (int)value * indentMultiplier;
                return indent;
            }

            return indent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
