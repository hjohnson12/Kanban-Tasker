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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanTasker.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        private string appClientId;
        private string[] scopes = new string[] { "files.readwrite"};
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private AuthProvider authProvider;
        public IPublicClientApplication MsalClient { get; }

        public SettingsView()
        {
            this.InitializeComponent();

            MsalClient = PublicClientApplicationBuilder.Create(appId)
                            .WithAuthority("https://login.microsoftonline.com/common")
                            .WithLogging((level, message, containsPii) =>
                            {
                                Debug.WriteLine($"MSAL: {level} {message} ");
                            }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                            .WithUseCorporateNetwork(true)
                            .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                            .Build();

            //authProvider = new AuthProvider(appId, scopes);
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

        private async void btnBackupDb_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;

            //var authProvider = new AuthProvider(appId, scopes);
            //var authResult2 = authProvider.Login();
            //var authResult = GraphHelper.Login();

            //       var userr = GraphHelper.GetMeAsync().Result;
            //        var graphClient = new GraphServiceClient(authProvider);
            //        var user = await graphClient.Me
            //.Request()
            //.GetAsync();
            //var children = await graphClient.Me.Drive.Root.Children
            //                .Request()
            //                .GetAsync();

            //foreach (var child in children)
            //{
            //    var test = child.Name;
            //}

            // Works
            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.            
            //IEnumerable<IAccount> accounts = await MsalClient.GetAccountsAsync().ConfigureAwait(false);
            //IAccount firstAccount = accounts.FirstOrDefault();

            //try
            //{
            //    authResult = await MsalClient.AcquireTokenSilent(scopes, firstAccount)
            //                                      .ExecuteAsync();
            //}
            //catch (MsalUiRequiredException ex)
            //{
            //    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
            //    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

            //    try
            //    {
            //        authResult = await MsalClient.AcquireTokenInteractive(scopes)
            //                                          .ExecuteAsync()
            //                                          .ConfigureAwait(false);
            //    }
            //    catch (MsalException msalex)
            //    {
            //        await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await DisplayMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
            //    return;
            //}

            //if (authResult != null)
            //{
            //    // Backup to OneDrive

            //    //var content = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken).ConfigureAwait(false);
            //    // Go back to the UI thread to make changes to the UI
            //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //    {
            //        txtResults.Text = "THERES A RESULT";
            //        //DisplayBasicTokenInfo(authResult);
            //        //this.SignOutButton.Visibility = Visibility.Visible;
            //    });
            //}

            // KEEP

            // Initialize the auth provider with values
            var authProvider = new AuthProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = await authProvider.GetAccessToken();
            var graphClient = new GraphServiceClient(authProvider);
            var children = await graphClient.Me.Drive.Root.Children
                            .Request()
                            .GetAsync();

            // Initialize Graph Client
            GraphHelper.Initialize(authProvider);

            // didn't return correctly
            //// Signed-in user
            //var user = Task.Run(() => GraphHelper.GetMeAsync()).Result;
            var user = await GraphHelper.GetMeAsync();

            //var test = user.AboutMe;

            //var provider = ProviderManager.Instance.GlobalProvider;

            //if (provider != null && provider.State == ProviderState.SignedIn)
            //{
            //    // Do graph call here with provider.Graph...
            //    //var graphClient = provider.Graph;
            //    //var children = await graphClient.Me.Drive.Root.Children
            //    //                .Request()
            //    //                .GetAsync();

            //    //foreach (var child in children)
            //    //{
            //    //    var test = child.Name;
            //    //}

            //    // Search for folder inside of OneDrive
            //    //var search = await graphClient.Me.Drive.Root
            //    //    .Search("Documents")
            //    //    .Request()
            //    //    .GetAsync();
            //    //            var driveItem = await graphClient.Me.Drive.Root
            //    //.Request()
            //    //.GetAsync();
            //    //var stream = "The contents of the file goes here.";

            //    //await graphClient.Me.Drive.Items["test.txt"].Content
            //    //    .Request()
            //    //    .PutAsync(stream);
            //    //var test = children.Count;

            //    // Create new folder in OneDrive Root Folder
            //    //            var driveItem = new Microsoft.Graph.DriveItem
            //    //            {
            //    //                Name = "KanbanTasker",
            //    //                Folder = new Microsoft.Graph.Folder
            //    //                {
            //    //                },
            //    //                AdditionalData = new Dictionary<string, object>()
            //    //{
            //    //    {"@microsoft.graph.conflictBehavior","rename"}
            //    //}
            //    //            };

            //    //            await graphClient.Me.Drive.Root.Children
            //    //                .Request()
            //    //                .AddAsync(driveItem);
            //}
            //else
            //{
            //    //Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification message = new Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification();
            //    //message.Show("Login failed. Please try again.", 3000);
            //}
        }

        private async void btnRestoreDb_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await MsalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await MsalClient.RemoveAsync(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    txtResults.Text = "User has signed-out";
                    //this.CallGraphButton.Visibility = Visibility.Visible;
                    //this.SignOutButton.Visibility = Visibility.Collapsed;
                });
            }
            catch (MsalException ex)
            {
                txtResults.Text = $"Error signing-out user: {ex.Message}";
            }
        }


        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       txtResults.Text = message;
                   });
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        //private async Task<string> GetHttpContentWithToken(string url, string token)
        //{
        //    var httpClient = new System.Net.Http.HttpClient();
        //    System.Net.Http.HttpResponseMessage response;
        //    try
        //    {
        //        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
        //        //Add the token in Authorization header
        //        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //        response = await httpClient.SendAsync(request).ConfigureAwait(false);
        //        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        return content;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //}
    }
}
