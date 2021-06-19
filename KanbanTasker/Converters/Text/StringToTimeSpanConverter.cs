using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanbanTasker.Extensions;
using Windows.UI.Xaml.Data;

namespace KanbanTasker.Converters.Text
{
    public class StringToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            // When using on TimePicker, it doesn't accept nullable timespan
            // Setting to current time on default since there is no null
            TimeSpan ts = DateTime.Now.TimeOfDay;

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
