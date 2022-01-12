using KanbanTasker.Extensions;
using KanbanTasker.Helpers.Microsoft_Graph.Requests;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace KanbanTasker.Helpers.Microsoft_Graph
{
    /// <summary>
    /// A helper class to interact with the Microsoft Graph SDK.
    /// </summary>
    public class GraphServiceHelper
    {
        public GraphServiceClient GraphServiceClient { get; set; }
        public OneDriveRequests OneDrive { get; set; }
        public UserRequests User { get; set; }

        public GraphServiceHelper(IAuthenticationProvider authenticationProvider)
        {
            // Initialize GraphServiceClient for use in the request classes
            GraphServiceClient = new GraphServiceClient(authenticationProvider);

            // Initialize graph request classes with client before use
            OneDrive = new OneDriveRequests(GraphServiceClient);
            User = new UserRequests(GraphServiceClient);
        }
    }
}