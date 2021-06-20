using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    public interface IAppNotificationService
    {
        void DisplayNotificationAsync(string message, int duration);
    }
}
