using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Autofac;
using KanbanTasker.Services;
using KanbanTasker.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KanbanTasker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        public MainViewModel ViewModel { get; set; }
        private FlyoutBase ActiveFlyout;
        
        public MainView()
        {
            this.InitializeComponent();

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            var appNotificationService = App.container.Resolve<IAppNotificationService>();

            ViewModel = App.GetViewModel(contentFrame, appNotificationService);
        }

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsView();
            CloseAllOpenPopups();
            await dialog.ShowAsync();
        }
       
        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            ActiveFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void ShowDialog(object sender, RoutedEventArgs e)
        {
            // This function shows the dialog used for Creating/Editing a board
            // Hides the EditConfirmationFlyout if user chose to edit board
            if (ActiveFlyout != null)
                ActiveFlyout.Hide();
            CloseAllOpenPopups();

            var dialog = new EditBoardDialogView(ViewModel);
            await dialog.ShowAsync();
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

        private async void btnOpenTaskCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentBoard != null)
            {
                CloseAllOpenPopups();

                var dialog = new CalendarDialogView(ViewModel);
                await dialog.ShowAsync();
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
    }
}