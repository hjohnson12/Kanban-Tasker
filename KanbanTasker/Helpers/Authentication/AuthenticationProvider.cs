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

namespace KanbanTasker.Helpers.Authentication
{
    /// <summary>
    /// Authentication Provider for Microsoft Graph SDK
    /// </summary>
    public class AuthenticationProvider : IAuthenticationProvider
    {
        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;
        private AuthenticationResult authResult { get; set; }

        public AuthenticationProvider(string appId, string[] scopes)
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
                    authResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                                                      .ExecuteAsync();
                    _userAccount = authResult.Account;
                    return authResult.AccessToken;
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                    // This indicates you need to call AcquireTokenAsync to acquire a token
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

                return authResult == null ? "" : authResult.AccessToken;
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

        /// <summary>
        /// Required function to implement IAuthenticationProvider. 
        /// <para>The Graph SDK will call this function each time it makes a Graph call.</para>
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }

        /// <summary>
        /// Sign the current user out and remove its access token.
        /// </summary>
        /// <returns></returns>
        public async Task SignOut()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await _msalClient.RemoveAsync(firstAccount).ConfigureAwait(false);
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    txtResults.Text = "User has signed-out";
                //    //this.CallGraphButton.Visibility = Visibility.Visible;
                //    //this.SignOutButton.Visibility = Visibility.Collapsed;
                //});
            }
            catch (MsalException ex)
            {
                // txtResults.Text = $"Error signing-out user: {ex.Message}";
            }
        }
    }
}
