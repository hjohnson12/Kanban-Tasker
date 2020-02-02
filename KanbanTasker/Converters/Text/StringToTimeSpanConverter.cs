using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanbanTasker.Helpers.Extensions;
using Windows.UI.Xaml.Data;

namespace KanbanTasker.Converters.Text
{
    public class StringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan ts;

            if (value != null && value is string)
            {
                var stringToConvert = value as string;
                bool success = TimeSpan.TryParse(stringToConvert, out ts);
                if (success)
                    return ts;
            }
            return ts;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string time;
            if (value != null && value is DateTimeOffset)
            {
                var valueToConvert = (DateTimeOffset)value;
                time = new DateTime(valueToConvert.Ticks).ToString("yyyy.MM.dd");
            }
            else
                time = string.Empty;
            return time;
        }
    }
}
