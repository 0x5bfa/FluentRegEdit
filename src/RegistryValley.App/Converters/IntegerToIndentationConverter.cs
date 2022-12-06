using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RegistryValley.App.Converters
{
    public class IntegerToIndentationConverter : IValueConverter
    {
        private int indentMultiplier;
        public int IndentMultiplier { get => indentMultiplier; set => indentMultiplier = value; }

        public IntegerToIndentationConverter()
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
