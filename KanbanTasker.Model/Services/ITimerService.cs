using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    public interface ITimerService
    {
        void ConfigureTimer();
        void StartTimer();
        void StopTimer();
    }
}
