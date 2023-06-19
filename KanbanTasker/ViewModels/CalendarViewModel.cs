using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanbanTasker.Base;
using KanbanTasker.Extensions;
using KanbanTasker.Models;
using KanbanTasker.Model.Services;

namespace KanbanTasker.ViewModels
{
    public class CalendarViewModel : Observable
    {
        private readonly ITimerService _timerService;
        private DateTime _currentTime;
        private DateTimeOffset _selectedDate;
        private ObservableCollection<PresentationTask> _scheduledTasks;
        private bool _isResultsVisible;

        public CalendarViewModel(ITimerService timerService)
        {
            _timerService = timerService;
            _timerService.Tick += Timer_Tick;

            CurrentTime = DateTime.Now;
            SelectedDate = DateTimeOffset.Now;

            StartTimer();
        }

        private void Timer_Tick(ITimerService obj)
        {
            CurrentTime = DateTime.Now;
        }

        public DateTime CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public ObservableCollection<PresentationTask> ScheudledTasks
        {
            get => _scheduledTasks;
            set
            {
                SetProperty(ref _scheduledTasks, value);
                //OnPropertyChanged();
                OnPropertyChanged("IsResultsVisible");
            }
        }

        public bool IsResultsVisible
        {
            get => _scheduledTasks.Count == 0;
            set => _isResultsVisible = value;
        }

        private void StartTimer() => _timerService.Start();

        public void StopTimer() => _timerService.Stop();

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
    }
}