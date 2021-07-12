using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace KanbanTasker.Converters.Text
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If true, a task is passed due and a red brush is needed
            // Otherwise, use the normal brush
            bool isPassedDue = (bool)value;
            if (isPassedDue)
                return new SolidColorBrush(Windows.UI.Colors.Red) { Opacity = 0.6 };
            return Application.Current.Resources["RegionBrush"] as AcrylicBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
            //return Application.Current.Resources["RegionBrush"] as AcrylicBrush;
        }
    }
}