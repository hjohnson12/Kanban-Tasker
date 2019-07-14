using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tasker.Converters.Text
{
    public class CardCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null && parameter.Equals("SwimlaneCardCount"))
            {
                int itemsCount = (int)value;
                if (itemsCount == 0 || itemsCount == 1)
                {
                    return value + " " + "Item";
                }

                return value + " " + "Items";
            }

            ColumnTag columnTag = value as ColumnTag;
            if (columnTag == null)
            {
                return string.Empty;
            }

            if (columnTag.Minimum > -1 && columnTag.Maximum > -1)
            {
                return "  |  " + "Min:" + " " + columnTag.Minimum + " / "
                          + "Max:" + " " + columnTag.Maximum;
            }
            else if (columnTag.Maximum > -1)
            {
                return "  |  " + "Max:" + " " + columnTag.Maximum;

            }
            else if (columnTag.Minimum > -1)
            {
                return "  |  " + "Min:" + " " + columnTag.Minimum;

            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
