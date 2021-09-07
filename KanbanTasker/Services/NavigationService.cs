using System;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Services
{
    /// <summary>
    /// A class for handling navigation within a frame.
    /// </summary>
    public class NavigationService : INavigationService
    {
        public object Frame { get; set; }

        public void NavigateTo(Type type)
        {
            if (Frame is Frame frame)
            {
                frame.Navigate(type);
            }
        }

        public void NavigateTo(Type type, object parameter)
        {
            if (Frame is Frame frame)
            {
                frame.Navigate(type, parameter);
            }
        }
    }
}