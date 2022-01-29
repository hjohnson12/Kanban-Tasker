using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    public interface ITimerService
    {
        event Action<ITimerService> Tick;
        void Start();
        void Stop();
        DateTime Time { get; set; }
    }
}
