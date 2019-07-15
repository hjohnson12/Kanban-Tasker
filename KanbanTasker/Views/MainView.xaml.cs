using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KanbanTasker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        public BoardViewModel ViewModel { get; set; }

        public MainView()
        {
            this.InitializeComponent();

            // var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            // Set the BackgroundElement instead of the entire Titlebar grid
            // so that we can add clickable element in title bar.
            Window.Current.SetTitleBar(AppTitleBar);

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            // Set active window colors
            titleBar.ForegroundColor = Windows.UI.Colors.White;
            titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.LightBlue;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.White;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

            ViewModel = new BoardViewModel();
        }
    }
}
