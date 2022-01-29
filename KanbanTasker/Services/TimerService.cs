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
        private DispatcherTimer _timer;

        public DateTime Time { get; set; }

        public void ConfigureTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick; ;
        }

        private void Timer_Tick(object sender, object e)
        {
            Time = DateTime.Now;
        }

        public void StartTimer()
        {
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }
    }
}