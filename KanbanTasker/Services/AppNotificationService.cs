using KanbanTasker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        public void DisplayNotificationAsync(string message, int duration)
        {
            var frame = (Frame)Window.Current.Content;
            (frame.Content as MainView).KanbanInAppNotification.Show(message, duration);
        }
    }
}
