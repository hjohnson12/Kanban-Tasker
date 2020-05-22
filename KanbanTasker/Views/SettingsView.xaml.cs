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

            if (App.CurrentUser != null)
                txtResults.Text = "Welcome " + App.CurrentUser.GivenName;
            else
            {
                txtResults.Text = "Welcome, please login to authenticate";
                btnSignOutTip.IsEnabled = false;
            }

            DataContext = ViewModel;

            // Get the Authentication Provider
            authProvider = App.GetAuthenticationProvider();
        }

        // UI Events / Methods
 
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

        private void btnRestoreTip_Click(object sender, RoutedEventArgs e)
        {
            RestoreTip.IsOpen = true;
        }

        private async void RestoreTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseTeachingTips();
            progressRing.IsActive = true;

            try
            {
                // Request a token to sign in the user
                var accessToken = await authProvider.GetAccessToken();

                // Initialize Graph Client
                GraphServiceHelper.Initialize(authProvider);

                // Set current user (temp)
                App.CurrentUser = await GraphServiceHelper.GetMeAsync();

                // Find the backupFolder in OneDrive, if it exists
                var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

                // Restore local data file using the backup file
                await GraphServiceHelper.RestoreFileFromOneDrive(backupFolder.Id, "ktdatabase.db");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            progressRing.IsActive = false;
            await DisplayMessageAsync("Data restored successfully.");

            // Debug results
            var displayName = await GraphServiceHelper.GetMyDisplayName();
            txtResults.Text = "Welcome " + App.CurrentUser.GivenName;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       btnSignOutTip.IsEnabled = true;
                   });

            // test
            await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("Application Restart Programmatic");
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

            // Request a token to sign in the user
            var accessToken = await authProvider.GetAccessToken();

            // Initialize Graph Client
            GraphServiceHelper.Initialize(authProvider);

            // Set current user (temp)
            App.CurrentUser = await GraphServiceHelper.GetMeAsync();
            
            // Find backupFolder in user's OneDrive, if it exists
            var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

            // Create backup folder in OneDrive if not exists
            if (backupFolder == null)
                backupFolder = await GraphServiceHelper.CreateNewOneDriveFolder("Kanban Tasker") as DriveItem;

            // Backup datafile (or overwrite)
            var uploadedFile = await GraphServiceHelper.UploadFileToOneDrive(backupFolder.Id, DataFilename);
            
            progressRing.IsActive = false;
            await DisplayMessageAsync("Data backed up successfully.");

            // Debug Results
            var displayName = await GraphServiceHelper.GetMyDisplayName();
            txtResults.Text = "Welcome, " + displayName;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       btnSignOutTip.IsEnabled = true;
                   });
        }

        private async void SignOutPopup_ConfirmClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (SignOutPopup.IsOpen)
                SignOutPopup.IsOpen = false;

            await authProvider.SignOut();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtResults.Text = "User has signed-out";
                this.btnSignOutTip.IsEnabled = false;
                App.CurrentUser = null;
            });
        }
    }
}
