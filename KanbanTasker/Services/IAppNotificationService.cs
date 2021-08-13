using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    /// <summary>
    /// Interface for displaying a message on the UI.
    /// </summary>
    public interface IAppNotificationService
    {
        void DisplayNotificationAsync(string message, int duration);
    }
}
