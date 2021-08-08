using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graph;
using System.Threading.Tasks;
using Autofac;
using KanbanTasker.ViewModels;
using KanbanTasker.Helpers.Microsoft_Graph.Authentication;
using KanbanTasker.Services;

namespace KanbanTasker.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
       
        public SettingsView()
        {
            this.InitializeComponent();

            DataContext = new SettingsViewModel(App.container.Resolve<IAppNotificationService>());
        }

        /// <summary>
        /// Displays a message using the InAppNotification in MainView. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       var frame = (Frame)Window.Current.Content;
                       (frame.Content as MainView).KanbanInAppNotification.Show(message, 5000);
                   });
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void btnBackupTip_Click(object sender, RoutedEventArgs e)
        {
            BackupTip.IsOpen = true;
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SignOutPopup.IsOpen = true;
        }

        private void SignOutPopup_ConfirmClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (SignOutPopup.IsOpen)
                SignOutPopup.IsOpen = false;
        }

        private void btnRestoreTip_Click(object sender, RoutedEventArgs e)
        {
            RestoreTip.IsOpen = true;
        }

        private void RestoreTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseTeachingTips();
        }

        private void BackupTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseTeachingTips();
        }

        public void CloseTeachingTips()
        {
            if (RestoreTip.IsOpen)
                RestoreTip.IsOpen = false;

            if (BackupTip.IsOpen)
                BackupTip.IsOpen = false;
        }
    }
}