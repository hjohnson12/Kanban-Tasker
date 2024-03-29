﻿using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Autofac;
using KanbanTasker.Services;
using KanbanTasker.ViewModels;
using KanbanTasker.Model.Services;

namespace KanbanTasker.Views
{
    public sealed partial class MainView : Page
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;
        private FlyoutBase ActiveFlyout;
        
        public MainView()
        {
            this.InitializeComponent();

            // Set XAML element as a draggable region
            Window.Current.SetTitleBar(AppTitleBar);

            var appNotificationService = App.container.Resolve<IAppNotificationService>();
            var dialogService = App.container.Resolve<IDialogService>();
            var navigationService = App.container.Resolve<INavigationService>();
            navigationService.Frame = contentFrame;

            DataContext = App.GetViewModel(navigationService, appNotificationService, dialogService);
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            ActiveFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ShowDialog(object sender, RoutedEventArgs e)
        {
            // Shows the dialog used for creating/editing a board
            // Hides the EditConfirmationFlyout if user chose to edit board
            if (ActiveFlyout != null)
                ActiveFlyout.Hide();
        }

        private void HideFlyout(object sender, RoutedEventArgs e)
        {
            if (ActiveFlyout == null)
                return;

            ActiveFlyout.Hide();
        }

        private async void BtnCompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();

            if (view.ViewMode == ApplicationViewMode.Default)
            {
                btnCompactOverlay.Icon = new SymbolIcon((Symbol)0xE944);
                btnOpenTaskCalendar.IsEnabled = false;
                await view.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
            else
            {
                btnCompactOverlay.Icon = new SymbolIcon((Symbol)0xE8A7);
                btnOpenTaskCalendar.IsEnabled = true;
                await view.TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
        }

        /// <summary>
        /// Closes all open content dialogs in the Visual Tree
        /// </summary>
        private static void CloseAllOpenPopups()
        {
            var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in popups)
            {
                if (popup.Child is ContentDialog dialog)
                {
                    dialog.Hide();
                }
            }
        }

        private void kanbanNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
           
        }
    }
}