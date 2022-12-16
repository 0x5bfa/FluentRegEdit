using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace RegistryValley.App.Converters
{
    class StringToSurroundedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string strParam && strParam.Length == 2)
            {
                return $"{strParam.First()}{value.ToString().Trim()}{strParam.Last()}";
            }
            else
            {
                return value.ToString().Trim();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
