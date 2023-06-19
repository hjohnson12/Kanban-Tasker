using System;
using System.Text;
using Windows.UI.Xaml;

namespace KanbanTasker.Helpers
{
    public static class CalendarViewHelper
    {
        public static DateTimeOffset GetSelectedDate(DependencyObject obj)
        {
            return (DateTimeOffset)obj.GetValue(SelectedDateProperty);
        }

        public static void SetSelectedDate(DependencyObject obj, DateTimeOffset value)
        {
            obj.SetValue(SelectedDateProperty, value);
        }

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.RegisterAttached("SelectedDate", typeof(DateTimeOffset), typeof(Windows.UI.Xaml.Controls.CalendarView),
                new PropertyMetadata(null, (d, e) =>
                {
                    var calendarView = (Windows.UI.Xaml.Controls.CalendarView)d;
                    var newSelectedDate = (DateTimeOffset)e.NewValue;

                    if (!calendarView.SelectedDates.Contains(newSelectedDate))
                    {
                        calendarView.SelectedDates.Clear();
                        calendarView.SelectedDates.Add(newSelectedDate);
                    }
                }));
    }
}