using KanbanTasker.Helpers.MicrosoftGraph;
using KanbanTasker.Helpers.MicrosoftGraph.Authentication;
using KanbanTasker.Helpers.MicrosoftGraph.Requests;

namespace KanbanTasker.Services
{
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
    }
}