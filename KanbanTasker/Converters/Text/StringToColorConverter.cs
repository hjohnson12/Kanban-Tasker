using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace KanbanTasker.Converters.Text
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString() == "Low")
            {

                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Green,
                    FallbackColor = Colors.Green,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            if (value.ToString() == "Normal")
            {
                // Normal Priority - Reveal Brush
                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Orange,
                    FallbackColor = Colors.Orange,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            if (value.ToString() == "High")
            {
                // High Priority - Reveal Brush
                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Red,
                    FallbackColor = Colors.Red,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
