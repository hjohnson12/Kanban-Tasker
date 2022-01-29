using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    public interface ITimerService
    {
        DateTime Time { get; set; }
        void Start();
        void Stop();
    }
}
