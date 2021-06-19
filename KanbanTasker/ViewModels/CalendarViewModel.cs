using KanbanTasker.Base;
using KanbanTasker.Helpers.Extensions;
using KanbanTasker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace KanbanTasker.ViewModels
{
    public class CalendarViewModel : Observable
    {
        private DispatcherTimer timer;
        private DateTime _currentTime;
        private DateTimeOffset _selectedDate;
        private ObservableCollection<PresentationTask> _scheduledTasks;
        private bool _isResultsVisible;

        public CalendarViewModel()
        {
            SelectedDate = DateTimeOffset.Now;
            CurrentTime = DateTime.Now;
            StartTimer();
        }
       
        public DateTime CurrentTime
        {
            get => _currentTime;
            set
            {
                if(_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<PresentationTask> ScheudledTasks
        {
            get => _scheduledTasks;
            set
            {
                if(_scheduledTasks != value)
                {
                    _scheduledTasks = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsResultsVisible");
                }
            }
        }

        public bool IsResultsVisible
        {
            get
            {
                return _scheduledTasks.Count == 0;
            }
            set
            {
                _isResultsVisible = value;
            }
        }

        /// <summary>
        /// Gets all of the tasks from the current board and places only the ones
        /// due today in the collection. Sorts by time due before returning.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <returns></returns>
        public ObservableCollection<PresentationTask> GetAvailableTasks(PresentationBoard currentBoard)
        {
            ScheudledTasks = new ObservableCollection<PresentationTask>();
            // Get all tasks for the current day
            if (currentBoard.Tasks != null && currentBoard.Tasks.Any())   // hack
                foreach (PresentationTask task in currentBoard.Tasks)
                {
                    if (!string.IsNullOrEmpty(task.DueDate))
                    {
                        var dueDate = task.DueDate.ToNullableDateTimeOffset();
                        if (dueDate.Value.Year == SelectedDate.Year && 
                            dueDate.Value.Month == SelectedDate.Month &&
                            dueDate.Value.Day == SelectedDate.Day)
                        {
                            ScheudledTasks.Add(task);
                        }
                    }
                }
            return new ObservableCollection<PresentationTask>(ScheudledTasks.OrderBy(x => x.TimeDue));
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_tick;
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        private void timer_tick(object sender, object e)
        {
            CurrentTime = DateTime.Now;
        }
    }
}
