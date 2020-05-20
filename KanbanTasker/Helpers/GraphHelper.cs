using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Helpers
{
    public class GraphHelper
    {
        public static GraphServiceClient GraphClient { get; set; }
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            GraphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<User> GetMeAsync()
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

        public static async Task<DriveItem> GetUsersOneDriveRoot()
        {
            try
            {
                // GET /me/drive/root ???
                return await GraphClient.Me.Drive.Root.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in users one drive root: {ex.Message}");
                return null;
            }
        }

        // <GetEventsSnippet>
        public static async Task<IEnumerable<Event>> GetEventsAsync()
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
                Console.WriteLine($"Error getting events: {ex.Message}");
                return null;
            }
        }
        // </GetEventsSnippet>
    }
}
