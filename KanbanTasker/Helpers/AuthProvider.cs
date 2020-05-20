using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Helpers
{
    public class AuthProvider : IAuthenticationProvider
    {
        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;
        private AuthenticationResult authResult { get; set; }

        public AuthProvider(string appId, string[] scopes)
        {
            _scopes = scopes;

            //_msalClient = PublicClientApplicationBuilder
            //    .Create(appId)
            //    .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, true)
            //    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            //    .Build();

            _msalClient = PublicClientApplicationBuilder.Create(appId)
                .WithAuthority("https://login.microsoftonline.com/common")
                .WithLogging((level, message, containsPii) =>
                {
                    Debug.WriteLine($"MSAL: {level} {message} ");
                }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .WithUseCorporateNetwork(true)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();

            authResult = null;

            //_msalClient = PublicClientApplicationBuilder
            //    .Create(appId)
            //    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            //    .Build();
        }

        public async Task<string> GetAccessToken()
        {
            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.            
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();
            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Invoke device code flow so user can sign-in with a browser
                    //var result = await _msalClient.AcquireTokenWithDeviceCode(_scopes, callback => {
                    //    Console.WriteLine(callback.Message);
                    //    return Task.FromResult(0);
                    //}).ExecuteAsync();

                    //_userAccount = result.Account;

                    authResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                                                      .ExecuteAsync();
                    _userAccount = authResult.Account;
                    return authResult.AccessToken;
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                    try
                    {
                        authResult = await _msalClient.AcquireTokenInteractive(_scopes)
                                                          .ExecuteAsync();
                    }
                    catch (MsalException msalex)
                    {
                        var test = "";
                        return null;
                        //await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }

                //if (authResult != null)
                //{
                //    // Backup to OneDrive

                //    //var content = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken).ConfigureAwait(false);
                //    // Go back to the UI thread to make changes to the UI
                //    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //    //{
                //    //    ResultText.Text = content;
                //    //    DisplayBasicTokenInfo(authResult);
                //    //    this.SignOutButton.Visibility = Visibility.Visible;
                //    //});
                //}
                return authResult == null ? "" : authResult.AccessToken;
                //catch (Exception exception)
                //{
                //    Console.WriteLine($"Error getting access token: {exception.Message}");
                //    return null;
                //}

                //try
                //{
                //    authResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                //                                      .ExecuteAsync();

                //}
                //catch (MsalUiRequiredException ex)
                //{
                //    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                //    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                //    try
                //    {
                //        authResult = await _msalClient.AcquireTokenInteractive(_scopes)
                //                                          .ExecuteAsync()
                //                                          .ConfigureAwait(false);
                //    }
                //    catch (MsalException msalex)
                //    {
                //        var test = "";
                //        //await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    // await DisplayMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                //    //return;
                //}

                //if (authResult != null)
                //{
                //    // Backup to OneDrive
                //    return authResult;


                //    //var content = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken).ConfigureAwait(false);
                //    // Go back to the UI thread to make changes to the UI
                //    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //    //{
                //    //    ResultText.Text = content;
                //    //    DisplayBasicTokenInfo(authResult);
                //    //    this.SignOutButton.Visibility = Visibility.Visible;
                //    //});
                //}
            }
            else
            {
                // If there is an account, call AcquireTokenSilent
                // By doing this, MSAL will refresh the token automatically if
                // it is expired. Otherwise it returns the cached token.

                var result = await _msalClient
                    .AcquireTokenSilent(_scopes, _userAccount)
                    .ExecuteAsync();

                return result.AccessToken;
            }


        }

        public async Task<AuthenticationResult> Login()
        {
            AuthenticationResult authResult = null;

            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.            
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                                                  .ExecuteAsync().ConfigureAwait(false);

            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await _msalClient.AcquireTokenInteractive(_scopes)
                                                      .ExecuteAsync()
                                                      .ConfigureAwait(false);
                }
                catch (MsalException msalex)
                {
                    var test = "";
                    //await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalex}");
                }
            }
            catch (Exception ex)
            {
                // await DisplayMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                //return;
            }

            if (authResult != null)
            {
                // Backup to OneDrive
                return authResult;


                //var content = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken).ConfigureAwait(false);
                // Go back to the UI thread to make changes to the UI
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    ResultText.Text = content;
                //    DisplayBasicTokenInfo(authResult);
                //    this.SignOutButton.Visibility = Visibility.Visible;
                //});
            }
            return authResult;
        }
        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}
