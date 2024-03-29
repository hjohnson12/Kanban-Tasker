﻿using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace KanbanTasker.Helpers.MicrosoftGraph.Authentication
{
    /// <summary>
    /// Authentication Provider for Microsoft Graph SDK
    /// </summary>
    public class AuthenticationProvider : IAuthenticationProvider
    {
        private readonly IPublicClientApplication _msalClient;
        private readonly string[] _scopes;
        private IAccount _userAccount;
        private IAccount _accountToLogin;
        private SynchronizationContext _syncContext;

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
        }

        private AuthenticationResult AuthResult { get; set; }

        /// <summary>
        /// Gets or sets the associated context thread to show the Interactive 
        /// prompt on for WAM (i.e., on the UI thread)
        /// </summary>
        public SynchronizationContext SynchronizationContext
        {
            get => _syncContext;
            set => _syncContext = value;
        }

        public async Task<IAccount> GetSignedInUser()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            return accounts.FirstOrDefault();
        }

        public async Task<string> GetAccessToken()
        {
            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.            
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            _accountToLogin = accounts.FirstOrDefault();
            _userAccount = _accountToLogin;

            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Attempts to acquire access token for the account from the user token cache
                    AuthResult = await _msalClient.AcquireTokenSilent(_scopes, _accountToLogin)
                                                      .ExecuteAsync();
                    _userAccount = AuthResult.Account;
                    return AuthResult.AccessToken;
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                    // This indicates we need to call AcquireTokenAsync to acquire a token,
                    // consent, or re-sign-in (password expiration), or two-factor authentication

                    // Request for interactive window to allow the user to select 
                    // an account, which aquires a token for the scopes if sucessful
                    AuthResult = await Task.Run<AuthenticationResult>(async () =>
                    {
                        // Task.Run() will guarantee the given piece of code be executed on a separate thread pool.
                        // Used to simulate the scenario of running the prompt on the UI from a different thread.
                        return await ShowInteractivePrompt();
                    });
                }
                catch (Exception ex)
                {
                    Debug.Write("ERROR: " + ex.Message);
                    throw;
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
        /// Displays the interactive prompt for authentication in the UI thread using a
        /// syncrhonization context
        /// </summary>
        /// <returns></returns>
        private Task<AuthenticationResult> ShowInteractivePrompt()
        {
            var taskCompletionSource = new TaskCompletionSource<AuthenticationResult>();

            // Uses Post to show the interactive prompt on the UI thread of the given context
            _syncContext.Post(async (_) =>
            {
                try
                {
                    var authResult = await _msalClient.AcquireTokenInteractive(_scopes)
                                            .WithAccount(_accountToLogin)
                                            .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                                            .ExecuteAsync();
                    taskCompletionSource.SetResult(authResult);
                }
                catch (MsalClientException msalClientEx)
                {
                    taskCompletionSource.SetException(msalClientEx);
                }
            }, null);

            return taskCompletionSource.Task;
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
            IAccount accountToLogin = accounts.FirstOrDefault();

            if (accountToLogin != null)
            {
                await _msalClient.RemoveAsync(accountToLogin).ConfigureAwait(false);
                _userAccount = null;
            }
        }
    }
}