using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using KanbanTasker.ViewModels;
using System.Threading;
using KanbanTasker.Helpers.Microsoft_Graph.Authentication;
using KanbanTasker.Helpers.Microsoft_Graph;
using System.Net;
using KanbanTasker.Services;
using Autofac;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanTasker.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        // Properties
        private string[] scopes = new string[] { "files.readwrite"};
        private string appClientId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private AuthenticationProvider authProvider;
        public const string DataFilename = "ktdatabase.db";
        public const string BackupFolderName = "Kanban Tasker";
        public User CurrentUser { get; set; }
        public BoardViewModel CurrentViewModel { get; set; }
        public SettingsViewModel ViewModel { get; set; }
       
        public SettingsView()
        {
            this.InitializeComponent();

            ViewModel = new SettingsViewModel(App.container.Resolve<IAppNotificationService>());
            DataContext = ViewModel;

            // Get the Authentication Provider
            authProvider = App.GetAuthenticationProvider();
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
                       //SettingsAppNotification.Show(message, 3000);
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

        private async void SignOutPopup_ConfirmClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
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
            AuthenticationResult authResult = null;
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