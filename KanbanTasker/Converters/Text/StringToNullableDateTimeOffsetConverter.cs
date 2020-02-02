using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace KanbanTasker.Converters.Text
{
    public class StringToNullableDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset? dt = null;
            DateTimeOffset dt2;

            if (value != null && value is string)
            {
                var stringToConvert = value as string;
                bool success = DateTimeOffset.TryParse(stringToConvert, out dt2);
                if (success)
                    dt = dt2;
                else
                    return dt;
            }
            return dt;
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
