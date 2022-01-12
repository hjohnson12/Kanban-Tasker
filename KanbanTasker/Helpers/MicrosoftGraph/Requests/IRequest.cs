using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Helpers.MicrosoftGraph.Requests
{
    /// <summary>
    /// Interface for different types of requests through Microsoft Graph
    /// </summary>
    public interface IRequest
    {
        GraphServiceClient GraphClient { get; set; }
    }
}