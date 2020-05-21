using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KanbanTasker.Services.SQLite;
using KanbanTasker.Helpers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using System.Threading.Tasks;
using KanbanTasker.Helpers.Authentication;
using KanbanTasker.ViewModels;
using System.Threading;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanTasker.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        private string appClientId;
        private string[] scopes = new string[] { "files.readwrite"};
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private AuthenticationProvider authProvider;
        public IPublicClientApplication MsalClient { get; }
        public const string DataFilename = "ktdatabase.db";
        public const string BackupFolderName = "Kanban Tasker";
        public User CurrentUser { get; set; }
        public BoardViewModel CurrentViewModel { get; set; }
        public SettingsView()
        {
            this.InitializeComponent();
            txtResults.Text = "Not Logged In";

            // Initialize the Authentication  Provider
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void SettingsDialog_ViewUpdatesClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            var dialog = new AppUpdatedDialogView();
            var result = await dialog.ShowAsync();
        }

        private async void btnBackupTip_Click(object sender, RoutedEventArgs e)
        {
            BackupTip.IsOpen = true;
        }

        private async void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            await authProvider.SignOut();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtResults.Text = "User has signed-out";
                //this.CallGraphButton.Visibility = Visibility.Visible;
                //this.SignOutButton.Visibility = Visibility.Collapsed;
            });
        }


        /// <summary>
        /// Displays a message in the InAppNotification. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       var frame = (Frame)Window.Current.Content;
                       (frame.Content as MainView).KanbanInAppNotification.Show(message, 3000);
                       //SettingsAppNotification.Show(message, 3000);
                   });
        }

        private async void btnRestoreDb_Click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;

            // Initialize Authentication Provider
            authProvider = new AuthenticationProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = await authProvider.GetAccessToken();

            // Initialize Graph Client
            GraphServiceHelper.Initialize(authProvider);
            var graphClient = new GraphServiceClient(authProvider);
            var user = await GraphServiceHelper.GetMeAsync();
            var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

            await GraphServiceHelper.RestoreFileFromOneDrive(backupFolder.Id, "ktdatabase.db");

            // Debug Results
            progressRing.IsActive = false;
            var displayName = await GraphServiceHelper.GetMyDisplayName();
            txtResults.Text = "Welcome, " + displayName;
            await DisplayMessageAsync("Data restored successfully.");

            // test
            Thread.Sleep(4000);
            var result = await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("Application Restart Programmatic");

        }

        private void btnRestoreTip_Click(object sender, RoutedEventArgs e)
        {
            RestoreTip.IsOpen = true;
        }

        private async void RestoreTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseTeachingTips();

            progressRing.IsActive = true;

            // Initialize Authentication Provider
            authProvider = new AuthenticationProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = await authProvider.GetAccessToken();

            // Initialize Graph Client
            GraphServiceHelper.Initialize(authProvider);
            var graphClient = new GraphServiceClient(authProvider);
            var user = await GraphServiceHelper.GetMeAsync();
            var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

            await GraphServiceHelper.RestoreFileFromOneDrive(backupFolder.Id, "ktdatabase.db");

            // Debug Results
            progressRing.IsActive = false;
            var displayName = await GraphServiceHelper.GetMyDisplayName();
            txtResults.Text = "Welcome, " + displayName;
            await DisplayMessageAsync("Data restored successfully.");

            // test
            Thread.Sleep(4000);
            var result = await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("Application Restart Programmatic");
        }

        public void CloseTeachingTips()
        {
            // Close teaching tips
            if (RestoreTip.IsOpen)
                RestoreTip.IsOpen = false;
            if (BackupTip.IsOpen)
                BackupTip.IsOpen = false;
        }

        private async void BackupTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            AuthenticationResult authResult = null;
            CloseTeachingTips();
            progressRing.IsActive = true;

            // Initialize Authentication Provider
            authProvider = new AuthenticationProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = await authProvider.GetAccessToken();

            // Initialize Graph Client
            GraphServiceHelper.Initialize(authProvider);
            var graphClient = new GraphServiceClient(authProvider);
            var user = await GraphServiceHelper.GetMeAsync();
            var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

            // Create backup folder in OneDrive if not exists
            if (backupFolder == null)
                backupFolder = await GraphServiceHelper.CreateNewOneDriveFolder("Kanban Tasker") as DriveItem;

            // Backup datafile (or overwrite)
            var uploadedFile = await GraphServiceHelper.UploadFileToOneDrive(backupFolder.Id, DataFilename);

            // Debug Results
            var displayName = await GraphServiceHelper.GetMyDisplayName();
            txtResults.Text = "Welcome, " + displayName;
            await DisplayMessageAsync("Data backed up successfully.");

            progressRing.IsActive = false;
        }
    }
}
