using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TjMott.Writer.Model;

namespace TjMott.Writer.Views
{
    public class NoteIconVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is long?)
            {
                long? docId = value as long?;
                if (docId.HasValue)
                    return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
