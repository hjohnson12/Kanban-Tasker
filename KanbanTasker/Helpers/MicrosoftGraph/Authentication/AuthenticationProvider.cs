using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.System;

namespace KanbanTasker.Helpers.MicrosoftGraph.Authentication
{
    /// <summary>
    /// Authentication Provider for Microsoft Graph SDK
    /// </summary>
    public class AuthenticationProvider : IAuthenticationProvider
    {
        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;

        // DQ for the associated UI thread
        private DispatcherQueue _dispatcherQueue;

        private AuthenticationResult AuthResult { get; set; }

        public AuthenticationProvider(string appId, string[] scopes)
        {
            _scopes = scopes;

            string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();

            // The redirect uri needed to register
            string redirectUriWithWAM = $"ms-appx-web://microsoft.aad.brokerplugin/{sid}";

            // Configure MSAL client using Web Account Manager (WAM)
            _msalClient = PublicClientApplicationBuilder.Create(appId)
                                .WithBroker(true)
                                .WithRedirectUri(redirectUriWithWAM)
                                .Build();

            // OLD - With HostAuth and not WAM
            //_msalClient = PublicClientApplicationBuilder.Create(appId)
            //    .WithAuthority(Authority)
            //    .WithLogging((level, message, containsPii) =>
            //    {
            //        Debug.WriteLine($"MSAL: {level} {message} ");
            //    }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
            //    .WithUseCorporateNetwork(true)
            //    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            //    .Build();

            AuthResult = null;
        }


        public async Task<IAccount> GetSignedInUser()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();
            return firstAccount;
        }

        public async Task<string> GetAccessToken()
        {
            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.            
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            _userAccount = firstAccount;

            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Attempts to acquire access token for the account from the user token cache
                    AuthResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                                                      .ExecuteAsync();
                    _userAccount = AuthResult.Account;
                    return AuthResult.AccessToken;
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                    // This indicates you need to call AcquireTokenAsync to acquire a token,
                    // consent, or re-sign-in (password expiration), or two-factor authentication
                    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
                
                    try
                    {
                        // Request for interactive window to allow the user to select 
                        // an account, which aquires a token for the scopes if sucessful
                        AuthResult = await Task.Run<AuthenticationResult>(async () =>
                        {
                            // Task.Run() will guarantee the given piece of code be executed on a separate thread pool.
                            // Used to simulate the scenario of running the prompt on the UI from a different thread.
                            return await _dispatcherQueue.EnqueueAsync<AuthenticationResult>(async () =>
                            {
                                return await _msalClient.AcquireTokenInteractive(_scopes)
                                                            .WithAccount(firstAccount)
                                                            .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                                                            .ExecuteAsync();
                            });

                            // Above was called from non-UI thread, then executed the Interactive prompt on the UI thread,
                            // and then returned from that thread
                        });
                    }
                    catch (MsalException msalex)
                    {
                        if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                            Debug.WriteLine($"MsalException, Authentication Canceled:{System.Environment.NewLine}{msalex}");
                        if (msalex.ErrorCode == MsalError.AuthenticationFailed)
                            Debug.WriteLine($"MsalException, Authentication Failed:{System.Environment.NewLine}{msalex}");
                        else if (msalex.ErrorCode == MsalError.RequestTimeout)
                            Debug.WriteLine($"MsalException, Request Timeout:{System.Environment.NewLine}{msalex}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write("ERROR: " + ex.Message);
                    return null;
                }

                return AuthResult == null ? "" : AuthResult.AccessToken;
            }
            else // Account Exists
            {
                // Since there is an account, call AcquireTokenSilent.
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
        /// Sign the current user out and remove the cached access tokens.
        /// </summary>
        /// <returns></returns>
        public async Task SignOut()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                if (firstAccount != null)
                {
                    await _msalClient.RemoveAsync(firstAccount).ConfigureAwait(false);
                    _userAccount = null;
                }
            }
            catch (MsalException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MsalException, Error signing-out user: {ex.Message}");
                throw;
            }
        }

        public void SetDispatcherQueue(DispatcherQueue dq)
        {
            this._dispatcherQueue = dq;
        }
    }
}
