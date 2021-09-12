using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KanbanTasker.Views;
using KanbanTasker.Model.Services;

namespace KanbanTasker.Services
{
    /// <summary>
    /// Class for displaying a message on the UI using InAppNotification from Microsoft Toolkit
    /// </summary>
    public class AppNotificationService : IAppNotificationService
    {
        /// <summary>
        /// Display a message on-screen to notify the user of something
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="duration">Duration in milliseconds</param>
        public void DisplayNotificationAsync(string message, int duration)
        {
            var frame = (Frame)Window.Current.Content;

            if (frame != null)
                (frame.Content as MainView).KanbanInAppNotification.Show(message, duration);
        }
    }
}