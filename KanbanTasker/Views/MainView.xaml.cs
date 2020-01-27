using KanbanTasker.ViewModels;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

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
            ViewModel = App.GetViewModel(contentFrame, KanbanInAppNotification);
        }

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsView();
            var result = await dialog.ShowAsync();
        }

        // This is a hack - works for the moment but there are other ways to show / hide flyouts
        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            ActiveFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            //var dialog = new EditBoardDialogView(ViewModel);
            //var result = await dialog.ShowAsync();
        }

        private async void ShowDialog(object sender, RoutedEventArgs e)
        {
            //ActiveFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

            // This function shows the dialog used for Creating/Editing a board
            // Hides the EditConfirmationFlyout if user chose to edit board
            if (ActiveFlyout != null)
                ActiveFlyout.Hide();

            var dialog = new EditBoardDialogView(ViewModel);
            var result = await dialog.ShowAsync();
        }

        private void HideFlyout(object sender, RoutedEventArgs e)
        {
            if (ActiveFlyout == null)
                return;

            ActiveFlyout.Hide();
        }

        private async void BtnCompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            var view = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (view.ViewMode == ApplicationViewMode.Default)
            {
                btnCompactOverlay.Icon = new SymbolIcon((Symbol)0xE944);
                await view.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
            else
            {
                btnCompactOverlay.Icon = new SymbolIcon((Symbol)0xE8A7);
                await view.TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
        }
    }
}