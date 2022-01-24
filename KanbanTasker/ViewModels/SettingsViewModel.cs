using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using KanbanTasker.Base;
using KanbanTasker.Services;
using KanbanTasker.Model.Services;
using Windows.System;
using System.Threading;
using CommunityToolkit.Mvvm.Input;

namespace KanbanTasker.ViewModels
{
    public class SettingsViewModel : Observable
    {
        private readonly IAppNotificationService _appNotificationService;
        private readonly GraphService _graphService;
        private const int NOTIFICATION_DURATION = 3000;
        public const string DataFilename = "ktdatabase.db";
        public const string BackupFolderName = "Kanban Tasker";
        private string _welcomeText;
        private bool _isSignoutEnabled;
        private bool _isProgressRingActive = false;
        private bool _isBackupPopupOpen;
        private bool _isSignoutPopupOpen;
        private bool _isRestorePopupOpen;
        private SynchronizationContext _syncContext;

        public Microsoft.Graph.User CurrentUser { get; set; }

        public ICommand BackupDbCommand { get; set; }
        public ICommand RestoreDbCommand { get; set; }
        public ICommand SignOutCommand { get; set; }

        public SettingsViewModel(
            IAppNotificationService appNotificationService,
            GraphService graphService)
        {
            BackupDbCommand = new AsyncRelayCommand(BackupDb, () => true);
            RestoreDbCommand = new AsyncRelayCommand(RestoreDb, () => true);
            SignOutCommand = new AsyncRelayCommand(SignOut, () => IsSignoutEnabled);

            _appNotificationService = appNotificationService;
            _graphService = graphService;

            // Context to the original thread of the calling code (i.e., UI thread)
            _syncContext = SynchronizationContext.Current;
            _graphService.AuthenticationProvider.SetSynchronizationContext(_syncContext);

            if (App.CurrentUser != null)
            {
                WelcomeText = "Welcome " + App.CurrentUser.GivenName;
                IsSignoutEnabled = true;
            }
            else
            {
                WelcomeText = "Welcome, please select an option below and sign in when prompted";
                IsSignoutEnabled = false;
            }
        }

        public string WelcomeText
        {
            get => _welcomeText;
            set => SetProperty(ref _welcomeText, value);
        }

        public bool IsSignoutEnabled
        {
            get => _isSignoutEnabled;
            set => SetProperty(ref _isSignoutEnabled, value);
        }

        public bool IsProgressRingActive
        {
            get => _isProgressRingActive;
            set => SetProperty(ref _isProgressRingActive, value);
        }

        public bool IsBackupPopupOpen
        {
            get => _isBackupPopupOpen;
            set => SetProperty(ref _isBackupPopupOpen, value);
        }

        public bool IsSignoutPopupOpen
        {
            get => _isSignoutPopupOpen;
            set => SetProperty(ref _isSignoutPopupOpen, value);
        }

        public bool IsRestorePopupOpen
        {
            get => _isRestorePopupOpen;
            set => SetProperty(ref _isRestorePopupOpen, value);
        }

        public async Task ExecuteOperationAsync(Func<Task> OneDriveOperation)
        {
            try
            {
                await OneDriveOperation();
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive

                    // Empty all cached accounts / data to allow user to rety
                    await _graphService.AuthenticationProvider.SignOut();

                    DisplayNotification("Error 401. Access Denied. Please make sure you've logged\ninto OneDrive and your email at least once then try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    DisplayNotification("Error 404. Resource requested is not available.");
                }
                else if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    DisplayNotification("Error 409. Error backing up, issue retrieving backup folder. Please try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    DisplayNotification("Error 502. Bad Gateway.\nPlease check your internet connection and try again in a few.");
                }
                else if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    DisplayNotification("Error 503. Service unavailable due to high load or maintenance.\nPlease try again in a few.");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    DisplayNotification("General Exception. Please check your internet connection and try again in a few.");
                }
            }
            catch (MsalClientException msalex)
            {
                if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    DisplayNotification(msalex.Message);
                }
                else if (msalex.ErrorCode == MsalError.InvalidGrantError)
                {
                    // invalid_grant comes from no consent to needed scopes
                    DisplayNotification("Invalid access scopes, please contact the developer.");
                }
                else
                {
                    DisplayNotification(msalex.Message);
                }
            }
            catch (MsalUiRequiredException msalReqEx)
            {
                DisplayNotification(msalReqEx.Message);
            }
            catch (Exception ex)
            {
                DisplayNotification("Unexpected Error: " + ex.Message);
            }
        }

        public async Task BackupDb()
        {
            IsProgressRingActive = true;
            ClosePopups();

            // Currently only one way to backup but can determine 
            // based on a parameter to Backup() if more are added
            await ExecuteOperationAsync(BackupToOneDrive);
            IsProgressRingActive = false;
        }

        /// <summary>
        /// Initiate backup of data to OneDrive.
        /// </summary>
        private async Task BackupToOneDrive()
        {
            // Request a token to sign in the user
            var accessToken = await _graphService.AuthenticationProvider.GetAccessToken();

            // Set current user (temp)
            App.CurrentUser = await _graphService.User.GetMeAsync();

            // Find backupFolder in user's OneDrive, if it exists
            DriveItem backupFolder = await _graphService.OneDrive.GetFolderAsync("Kanban Tasker");

            // Create backup folder in OneDrive if not exists
            if (backupFolder == null)
                backupFolder = await _graphService.OneDrive.CreateNewFolderAsync("Kanban Tasker");

            // Backup datafile (or overwrite)
            DriveItem uploadedFile = await _graphService.OneDrive.UploadFileAsync(backupFolder.Id, DataFilename);

            DisplayNotification("Data backed up successfully");

            var displayName = await _graphService.User.GetMyDisplayNameAsync();
            WelcomeText = "Welcome " + displayName;
            IsSignoutEnabled = true;
        }

        public async Task RestoreDb()
        {
            IsProgressRingActive = true;
            ClosePopups();

            // Currently only one way to restore but can determine
            // based on parameter to Restore() if more are added
            await ExecuteOperationAsync(RestoreFromOneDrive);
            IsProgressRingActive = false;
        }

        /// <summary>
        /// Initiate restoration of data from OneDrive.
        /// <para>*Application restarts if finished successfully.</para>
        /// </summary>
        private async Task RestoreFromOneDrive()
        {
            // Request a token to sign in the user
            var accessToken = await _graphService.AuthenticationProvider.GetAccessToken();

            // Set current user (temp)
            App.CurrentUser = await _graphService.User.GetMeAsync();

            // Find the backupFolder in OneDrive, if it exists
            var backupFolder = await _graphService.OneDrive.GetFolderAsync("Kanban Tasker");

            if (backupFolder != null)
            {
                // Restore local data file using the backup file, if it exists
                await _graphService.OneDrive.RestoreFileAsync(backupFolder.Id, "ktdatabase.db");

                DisplayNotification("Data restored successfully");

                var displayName = await _graphService.User.GetMyDisplayNameAsync();
                WelcomeText = "Welcome " + App.CurrentUser.GivenName;
                IsSignoutEnabled = true;

                // Restart app to make changes
                await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("");
            }
            else
                DisplayNotification("No backup folder found to restore from.");
        }

        private async Task SignOut()
        {
            ClosePopups();

            await ExecuteOperationAsync(async () => 
            {
                await _graphService.AuthenticationProvider.SignOut();

                WelcomeText = "User has signed-out";
                IsSignoutEnabled = false;
                App.CurrentUser = null;
            });
        }

        /// <summary>
        /// Display a notification message to the user on the screen.
        /// </summary>
        /// <param name="message"></param>
        public void DisplayNotification(string message)
        {
            _appNotificationService.DisplayNotificationAsync(message, NOTIFICATION_DURATION);
        }

        public void ShowBackupPopup() => IsBackupPopupOpen = true;

        public void ShowRestorePopup() => IsRestorePopupOpen = true;

        public void ShowSignoutPopup() => IsSignoutPopupOpen = true;

        public void ClosePopups()
        {
            IsBackupPopupOpen = false;
            IsRestorePopupOpen = false;
            IsSignoutPopupOpen = false;
        }
    }
}