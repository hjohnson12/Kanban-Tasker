using KanbanTasker.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace KanbanTasker.Services
{
    public class TimerService : ITimerService
    {
        private readonly DispatcherTimer _timer;

        public TimerService()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick; ;
        }

        public DateTime Time { get; set; }

        private void Timer_Tick(object sender, object e)
        {
            Time = DateTime.Now;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}