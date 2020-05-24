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
using KanbanTasker.ViewModels;
using System.Threading;
using Autofac.Core;
using KanbanTasker.Helpers.Microsoft_Graph.Authentication;
using KanbanTasker.Helpers.Microsoft_Graph;
using System.Net;

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
        /// Displays a message using the InAppNotification in MainView. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       var frame = (Frame)Window.Current.Content;
                       (frame.Content as MainView).KanbanInAppNotification.Show(message, 4000);
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
                GraphServiceHelper.InitializeClient(authProvider);

                // Set current user (temp)
                App.CurrentUser = await GraphServiceHelper.GetMeAsync();

                // Find the backupFolder in OneDrive, if it exists
                var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

                if (backupFolder != null)
                {
                    // Restore local data file using the backup file, if it exists
                    await GraphServiceHelper.RestoreFileFromOneDriveAsync(backupFolder.Id, "ktdatabase.db");

                    await DisplayMessageAsync("Data restored successfully.");

                    // Debug results
                    var displayName = await GraphServiceHelper.GetMyDisplayNameAsync();
                    txtResults.Text = "Welcome " + App.CurrentUser.GivenName;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                           () =>
                           {
                               btnSignOutTip.IsEnabled = true;
                           });

                    // test - restart app 
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("Application Restart Programmatic");
                }
                else
                    await DisplayMessageAsync("No backup folder found to restore from.");
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive

                    // Empty all cached accounts / data to allow user to rety
                    await authProvider.SignOut();

                    await DisplayMessageAsync("Access Denied. Please make sure you've logged\ninto OneDrive and your email at least once.");
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await DisplayMessageAsync("Resource requested is not available.");
                }
                else if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // Todo: Retry rquest over a new HTTP connection
                    await DisplayMessageAsync("Service unavailable due to high load or maintenance.\nPlease try again in a few.");
                }
                else if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    await DisplayMessageAsync("Error backing up, issue retrieving backup folder. Please try again.");
                }
            }
            catch (MsalException msalex)
            {
                if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    await DisplayMessageAsync(msalex.Message);
                }
                else if (msalex.ErrorCode == MsalError.InvalidGrantError)
                {
                    // invalid_grant ErrorCode comes from no consent to needed scopes
                    await DisplayMessageAsync("Invalid access scopes, please contact the developer.");
                }

            }
            catch (Exception ex)
            {
                await DisplayMessageAsync(ex.Message);
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        public void CloseTeachingTips()
        {
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

            try
            {
                // Request a token to sign in the user
                var accessToken = await authProvider.GetAccessToken();

                // Initialize Graph Client
                GraphServiceHelper.InitializeClient(authProvider);

                // Set current user (temp)
                App.CurrentUser = await GraphServiceHelper.GetMeAsync();

                // Find backupFolder in user's OneDrive, if it exists
                var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("Kanban Tasker");

                // Create backup folder in OneDrive if not exists
                if (backupFolder == null)
                    backupFolder = await GraphServiceHelper.CreateNewOneDriveFolderAsync("Kanban Tasker") as DriveItem;

                // Backup datafile (or overwrite)
                var uploadedFile = await GraphServiceHelper.UploadFileToOneDriveAsync(backupFolder.Id, DataFilename);

                await DisplayMessageAsync("Data backed up successfully.");

                // Debug Results
                var displayName = await GraphServiceHelper.GetMyDisplayNameAsync();
                txtResults.Text = "Welcome " + displayName;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>
                       {
                           btnSignOutTip.IsEnabled = true;
                       });

            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive

                    // Empty all cached accounts / data to allow user to rety
                    await authProvider.SignOut();

                    await DisplayMessageAsync("Access Denied. Please make sure you've logged\ninto OneDrive and your email at least once.");
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await DisplayMessageAsync("Resource requested is not available.");
                }
                else if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    await DisplayMessageAsync("Service unavailable due to high load or maintenance.\nPlease try again in a few.");
                }
                else if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    await DisplayMessageAsync("Error backing up, issue retrieving backup folder. Please try again.");
                }
            }
            catch (MsalException msalex)
            {
                if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    await DisplayMessageAsync(msalex.Message);
                }
                else if (msalex.ErrorCode == MsalError.InvalidGrantError)
                {
                    // invalid_grant ErrorCode comes from no consent
                    // for the correct scopes (todo: add interactive retry)
                    await DisplayMessageAsync("Invalid access scopes, please contact the developer.");
                }
            }
            catch (Exception ex)
            {                
                await DisplayMessageAsync(ex.Message);
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        private async void SignOutPopup_ConfirmClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (SignOutPopup.IsOpen)
                SignOutPopup.IsOpen = false;
            try
            {
                await authProvider.SignOut();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    txtResults.Text = "User has signed-out";
                    this.btnSignOutTip.IsEnabled = false;
                    App.CurrentUser = null;
                });
            }
            catch (MsalException ex)
            {
                await DisplayMessageAsync(ex.Message);
            }
        }
    }
}