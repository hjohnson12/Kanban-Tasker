using KanbanTasker.Base;
using KanbanTasker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.ViewModels
{
    public class CalendarViewModel : Observable
    {
        private DateTimeOffset _selectedDate;
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

        private ObservableCollection<PresentationTask> _ScheduledTasks;
        public ObservableCollection<PresentationTask> ScheudledTasks
        {
            get => _ScheduledTasks;
            set
            {
                if(_ScheduledTasks != value)
                {
                    _ScheduledTasks = value;
                    OnPropertyChanged();
                }
            }
        }

        public CalendarViewModel()
        {
            SelectedDate = DateTimeOffset.Now;
        }

        public ObservableCollection<PresentationTask> GetAvailableTasks(string currentDate)
        { 
            // Get all tasks for the current day
            return new ObservableCollection<PresentationTask>();
        }
    }
}
