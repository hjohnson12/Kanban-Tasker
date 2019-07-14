using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KanbanBoardUWP.Converters.Text
{
    public class CardTemplateConverter : DependencyObject, IValueConverter
    {

        public object column
        {
            get { return (object)GetValue(columnProperty); }
            set { SetValue(columnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for column.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty columnProperty =
            DependencyProperty.Register("column", typeof(object), typeof(CardTemplateConverter), new PropertyMetadata(null));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //KanbanColumn column = (KanbanColumn)parameter;

            //if (column.IsExpanded)
            //{
            //    column.Template = (ControlTemplate)column.Resources["CollapsedTemplate"];
            //}
            //else
            //{
            //    column.Template = (ControlTemplate)column.Resources["DefaultKanbanHeaderTemplate"];
            //}

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            //KanbanColumn column = (KanbanColumn)parameter;

            //if (column.IsExpanded)
            //{
            //    column.Template = (ControlTemplate)column.Resources["CollapsedTemplate"];
            //}
            //else
            //{
            //    column.Template = (ControlTemplate)column.Resources["DefaultKanbanHeaderTemplate"];
            //}

            return value;
        }
    }
}
