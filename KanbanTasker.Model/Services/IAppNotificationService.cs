using System;

namespace KanbanTasker.Model.Services
{
    /// <summary>
    /// Interface for displaying a message to the user.
    /// </summary>
    public interface IAppNotificationService
    {
        void DisplayNotificationAsync(string message, int duration);
    }
}