using KanbanTasker.Helpers.MicrosoftGraph;
using KanbanTasker.Helpers.MicrosoftGraph.Authentication;
using KanbanTasker.Helpers.MicrosoftGraph.Requests;
using System;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    /// <summary>
    /// A class to interact with properties pertaining to parts of the Microsoft Graph API
    /// </summary>
    public class GraphService
    {
        private string[] scopes = new string[] { "files.readwrite", "user.read" };
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private static AuthenticationProvider _authProvider;
        private GraphServiceHelper _graphServiceHelper;

        public GraphService()
        {
            _authProvider = new AuthenticationProvider(appId, scopes);
            _graphServiceHelper = new GraphServiceHelper(_authProvider);
        }

        public AuthenticationProvider AuthenticationProvider
        {
            get => _authProvider;
        }

        public OneDriveRequests OneDrive
        {
            get => _graphServiceHelper.OneDrive;
        }

        public UserRequests User
        {
            get => _graphServiceHelper.User;
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        private async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);

                // Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}