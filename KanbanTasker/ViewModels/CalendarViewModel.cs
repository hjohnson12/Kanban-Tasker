﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using KanbanTasker.Base;
using KanbanTasker.Extensions;
using KanbanTasker.Models;

namespace KanbanTasker.ViewModels
{
    public class CalendarViewModel : Observable
    {
        private DispatcherTimer _timer;
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
            get => _scheduledTasks.Count == 0;
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
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_tick;
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        private void timer_tick(object sender, object e)
        {
            CurrentTime = DateTime.Now;
        }
    }
}