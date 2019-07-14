using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace KanbanTasker.Converters.Text
{
    public class CollapsedHeaderMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TextBlock textBlock = value as TextBlock;
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double top = textBlock.DesiredSize.Width - 10;
            Thickness thickness = new Thickness(-12, top, 0, 0);
            return thickness;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
