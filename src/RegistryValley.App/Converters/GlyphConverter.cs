﻿using Microsoft.UI.Xaml.Data;

namespace RegistryValley.App.Converters
{
    public class GlyphConverter : IValueConverter
    {
        public string ExpandedGlyph { get; set; }
        public string CollapsedGlyph { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isExpanded = value as bool?;

            if (isExpanded.HasValue && isExpanded.Value)
            {
                return ExpandedGlyph;
            }
            else
            {
                return CollapsedGlyph;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
