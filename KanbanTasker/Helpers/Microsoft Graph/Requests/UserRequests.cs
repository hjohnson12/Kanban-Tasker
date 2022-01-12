using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Helpers.Microsoft_Graph.Requests
{
    public class UserRequests : IRequest
    {
        public GraphServiceClient GraphClient { get; set; }

        public UserRequests(GraphServiceClient graphServiceClient)
        {
            // Initializes the client used to make calls to the Microsoft Graph API
            GraphClient = graphServiceClient;
        }

        /// <summary>
        /// Get the current user.
        /// </summary>
        /// <returns>A user object representing the current user.</returns>
        public async Task<Microsoft.Graph.User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await GraphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the current user's email address from their profile.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetMyEmailAddressAsync()
        {
            // Get the current user. 
            // The app only needs the user's email address, so select the mail and userPrincipalName properties.
            // If the mail property isn't defined, userPrincipalName should map to the email for all account types. 
            Microsoft.Graph.User me = await GraphClient.Me.Request().Select("mail,userPrincipalName").GetAsync();
            return me.Mail ?? me.UserPrincipalName;
        }

        /// <summary>
        /// Get the current user's display name from their profile.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetMyDisplayNameAsync()
        {
            // Get the current user. 
            // The app only needs the user's displayName
            Microsoft.Graph.User me;
            try
            {
                me = await GraphClient.Me.Request().Select("displayName").GetAsync();
                return me.GivenName ?? me.DisplayName;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into personal site / OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Get events from the current user's calendar.
        /// </summary>
        /// <returns>Collection of events from a user's calendar.</returns>
        public async Task<IEnumerable<Event>> GetEventsAsync()
        {
            try
            {
                // GET /me/events
                var resultPage = await GraphClient.Me.Events.Request()
                    // Only return the fields used by the application
                    .Select(e => new {
                        e.Subject,
                        e.Organizer,
                        e.Start,
                        e.End
                    })
                    // Sort results by when they were created, newest first
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting events: {ex.Message}");
                return null;
            }
        }
    }
}