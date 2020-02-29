using KanbanTasker.Base;
using KanbanTasker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Windows.Input;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Syncfusion.UI.Xaml.Kanban;
using Microsoft.Toolkit.Uwp.UI.Controls;
using KanbanTasker.Helpers.Extensions;
using KanbanTasker.Helpers;
using LeaderAnalytics.AdaptiveClient;

namespace KanbanTasker.ViewModels
{
    public class BoardViewModel : Observable
    {
        /// <summary>
        /// Variables/Private backing fields
        /// </summary>
        ///
        private PresentationBoard _Board;
        public PresentationBoard Board
        {
            get => _Board;
            set
            {
                _Board = value;
                OnPropertyChanged();
            }
        }
        private PresentationTask _CurrentTask;   
        public PresentationTask CurrentTask
        {
            get => _CurrentTask;
            set
            {
                _CurrentTask = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SuggestedTagsCollection 
        {
            get => _suggestedTagsCollection;
            set
            {
                _suggestedTagsCollection = value;
                OnPropertyChanged();
            }
        }


        private string _paneTitle;
        private bool _isPointerEntered = false;
        private bool _isEditingTask;
        private string _currentCategory;
        private IAdaptiveClient<IServiceManifest> DataProvider;
        public ICommand NewTaskCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand SaveTaskCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }
        public ICommand DeleteTagCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand RemoveScheduledNotificationCommand { get; set; }

        #region Properties

        /// <summary>
        /// Used to fill indicator key combo box
        /// </summary>
        /// 
        private ObservableCollection<ComboBoxItem> _ColorKeys;
        public ObservableCollection<ComboBoxItem> ColorKeys
        {
            get { return _ColorKeys; }
            set
            {
                _ColorKeys = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ComboBoxItem> _ReminderTimes;
        public ObservableCollection<ComboBoxItem> ReminderTimes
        {
            get { return _ReminderTimes; }
            set
            {
                _ReminderTimes = value;
                OnPropertyChanged();
            }
        }

        
        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Used to determine if pointer entered
        /// inside of a card
        /// </summary>
        public bool IsPointerEntered
        {
            get { return _isPointerEntered; }
            set
            {
                _isPointerEntered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Used to display Edit/New text on splitview pane
        /// </summary>
        public bool IsEditingTask
        {
            get { return _isEditingTask; }
            set { _isEditingTask = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The category to be displayed on the edit/new task pane
        /// </summary>
        public string CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                OnPropertyChanged();
            }
        }

        public PresentationTask OriginalTask
        {
            get;
            set;
        }

        private InAppNotification MessagePump;
        private ObservableCollection<string> _suggestedTagsCollection;
        private const int MessageDuration = 3000;

        #endregion Properties

        /// <summary>
        /// Constructor / Initialization of tasks.
        /// </summary>
        public BoardViewModel(PresentationBoard board, IAdaptiveClient<IServiceManifest> dataProvider, InAppNotification messagePump)
        {
            Board = board;
            DataProvider = dataProvider;
            MessagePump = messagePump;
        
            CurrentTask = new PresentationTask(new TaskDTO());
            NewTaskCommand = new RelayCommand<ColumnTag>(NewTaskCommandHandler, () => true); // CanExecuteChanged is not working 
            EditTaskCommand = new RelayCommand<int>(EditTaskCommandHandler, () => true);
            SaveTaskCommand = new RelayCommand(SaveTaskCommandHandler, () => true);
            DeleteTaskCommand = new RelayCommand<int>(DeleteTaskCommandHandler, () => true);
            DeleteTagCommand = new RelayCommand<string>(DeleteTagCommandHandler, () => true);
            CancelEditCommand = new RelayCommand(CancelEditCommandHandler, () => true);
            RemoveScheduledNotificationCommand = new RelayCommand(RemoveScheduledNotficationCommandHandler, () => true);

            ColorKeys = new ObservableCollection<ComboBoxItem>();
            ColorKeys.Add(new ComboBoxItem { Content = "High" });
            ColorKeys.Add(new ComboBoxItem { Content = "Normal" });
            ColorKeys.Add(new ComboBoxItem { Content = "Low" });

            ReminderTimes = new ObservableCollection<ComboBoxItem>();
            ReminderTimes.Add(new ComboBoxItem { Content = "None" });
            ReminderTimes.Add(new ComboBoxItem { Content = "At Time of Due Date" });
            ReminderTimes.Add(new ComboBoxItem { Content = "5 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "10 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "15 Minutes Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "1 Hour Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "2 Hours Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "1 Day Before" });
            ReminderTimes.Add(new ComboBoxItem { Content = "2 Days Before" });

            if (Board.Tasks != null && board.Tasks.Any())   // hack
                foreach (PresentationTask task in Board.Tasks)
                {
                    task.ColorKeyComboBoxItem = GetComboBoxItemForColorKey(task.ColorKey);
                    task.ReminderTimeComboBoxItem = GetComboBoxItemForReminderTime(task.ReminderTime);
                }
        }

        private void RemoveScheduledNotficationCommandHandler()
        {
            ToastHelper.RemoveScheduledNotification(CurrentTask.ID.ToString());
            CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[0];
        }

        #region Functions

        public void NewTaskCommandHandler(ColumnTag tag)
        {
            PaneTitle = "New Task";
            string category = tag?.Header?.ToString();
            CurrentTask = new PresentationTask(new TaskDTO() { Category = category }) { Board = Board, BoardId = Board.ID,  ColorKeyComboBoxItem = ColorKeys[1], ReminderTimeComboBoxItem = ReminderTimes[0] };
            OriginalTask = null; 
            IsEditingTask = true;
        }

        public void EditTaskCommandHandler(int taskID)
        {
            PaneTitle = "Edit Task";
            CurrentTask = Board.Tasks.First(x => x.ID == taskID);
            IsEditingTask = true;
            InitializeTagAutosuggest();
            InitializeDateInformation();
            // clone a copy of CurrentTask so we can restore if user cancels
            OriginalTask = new PresentationTask(CurrentTask.To_TaskDTO());
        }

        private void InitializeTagAutosuggest()
        {
            SuggestedTagsCollection = Board.TagsCollection;
            foreach(var tag in CurrentTask.Tags)
            {
                if (SuggestedTagsCollection.Contains(tag))
                {
                    SuggestedTagsCollection.Remove(tag);
                }
                else
                    SuggestedTagsCollection = Board.TagsCollection;
            }
        }

        private void InitializeDateInformation()
        {
            DateTimeOffset? dateCreated = CurrentTask.DateCreated.ToNullableDateTimeOffset();
            DateTimeOffset? today = DateTimeOffset.Now;
            DateTimeOffset? startDate = CurrentTask.StartDate.ToNullableDateTimeOffset();
            DateTimeOffset? finishDate = CurrentTask.FinishDate.ToNullableDateTimeOffset();

            if (IsEditingTask)
            {
                // Days worked on
                if (finishDate != null && startDate != null)
                {
                    TimeSpan? ts = finishDate - startDate;

                    if (ts != null)
                        // Difference in days, hous, mins
                        CurrentTask.DaysWorkedOn = String.Format("{0}d, {1}hrs, {2}min",
                            ts.Value.Days.ToString(), ts.Value.Hours.ToString(), ts.Value.Minutes.ToString());
                }

                // Days since creation
                if (dateCreated != null && today != null)
                {
                    TimeSpan? ts = today - dateCreated;

                    if (ts != null)
                        // Difference in days, hours, mins
                        CurrentTask.DaysSinceCreation = String.Format("{0}d, {1}hrs, {2}min",
                            ts.Value.Days.ToString(), ts.Value.Hours.ToString(), ts.Value.Minutes.ToString());
                }
            }
        }

        public void SaveTaskCommandHandler()
        {
            IsEditingTask = false;

            if (CurrentTask == null)
                return;

            TaskDTO dto = CurrentTask.To_TaskDTO();
            dto.ColorKey = ((ComboBoxItem)CurrentTask.ColorKeyComboBoxItem)?.Content.ToString() ?? "Normal"; // hack
            dto.ReminderTime = ((ComboBoxItem)CurrentTask.ReminderTimeComboBoxItem)?.Content.ToString() ?? "None";

            bool isNew = dto.Id == 0;

            if (isNew)
            {
                dto.ColumnIndex = Board.Tasks?.Where(x => x.Category == dto.Category).Count() ?? 0;
                dto.DateCreated = DateTime.Now.ToString();
            }
            dto.Id = DataProvider.Call(x => x.TaskServices.SaveTask(dto)).Entity.Id;


            if (isNew)
            {
                CurrentTask.ID = dto.Id;
                CurrentTask.ColumnIndex = dto.ColumnIndex;
                Board.Tasks.Add(CurrentTask);
            }

            PrepareToastNotification();
            
            MessagePump.Show("Task was saved successfully", MessageDuration);
        }

        public void DeleteTaskCommandHandler(int taskID)
        {
            PresentationTask task = Board.Tasks.First(x => x.ID == taskID);
            RowOpResult result = DataProvider.Call(x => x.TaskServices.DeleteTask(taskID));

            if (result.Success)
            {
                Board.Tasks.Remove(task);
                CurrentTask = Board.Tasks.LastOrDefault();
                ToastHelper.RemoveScheduledNotification(taskID.ToString());
                int startIndex = task.ColumnIndex;

                // Calling OrderBy after Where, reordering a whole collection prior to filter is high overhead
                // If we do not sort by ColumnIndex, the tasks in Board.Tasks will be in unsorted order when assigning startIndex 

                // Questionable issue:
                //  Sometimes the task index value for a task in Board.Tasks are wrong, but correct in db (shouldn't be because of this though)
                //      Ex: We have a task named t2 which is index 2 in db (expected), but index 4 or something in Board.Tasks at time of deletion
                //      - I've only noticed it when moving a task and then deleting, sometimes... Possible binding issue when changing property, since db is correct? 
                //  otherTask.ColumnIndex -=1 works without OrderBy, but has the problem above
                // Fix: If an index in the db gets messed up, moving one card in the column fixes the whole column. Low severity.
                foreach (PresentationTask otherTask in Board.Tasks.Where(x => x.Category == task.Category && x.ColumnIndex > task.ColumnIndex).OrderBy(x => x.ColumnIndex)) 
                {
                    otherTask.ColumnIndex = startIndex++;
                    //otherTask.ColumnIndex -= 1;
                    UpdateCardIndex(otherTask.ID, otherTask.ColumnIndex);
                }
                MessagePump.Show("Task deleted from board successfully", MessageDuration);
            }
            else
                MessagePump.Show("Task failed to be deleted. Please try again or restart the application.", MessageDuration);
        }

        public void DeleteTagCommandHandler(string tag)
        {
            if (CurrentTask == null)
            {
                MessagePump.Show("Tag failed to be deleted.  CurrentTask is null. Please try again or restart the application.", MessageDuration);
                return;
            }
            CurrentTask.Tags.Remove(tag);
            MessagePump.Show("Tag deleted successfully", MessageDuration);
        }

        public void CancelEditCommandHandler()
        {
            IsEditingTask = false;

            if (OriginalTask == null)
                return;
            // roll back changes to CurrentTask
           else
            {
                int index = Board.Tasks.IndexOf(CurrentTask);
                Board.Tasks.Remove(CurrentTask);
                CurrentTask = new PresentationTask(OriginalTask.To_TaskDTO());
                Board.Tasks.Insert(index, CurrentTask);

                // Check if a toast notification was deleted
                if (OriginalTask.ReminderTime != "None")
                    PrepareToastNotification();

                // Reset combo box selected item since UWP Combobox doesn't bind correctly
                switch (OriginalTask.ColorKey)
                {
                    case "Low":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[0];
                        break;
                    case "Normal":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[1];
                        break;
                    case "High":
                        CurrentTask.ColorKeyComboBoxItem = ColorKeys[2];
                        break;
                }
                switch (OriginalTask.ReminderTime)
                {
                    case "None":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[0];
                        break;
                    case "At Time of Due Date":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[1];
                        break;
                    case "5 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[2];
                        break;
                    case "10 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[3];
                        break;
                    case "15 Minutes Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[4];
                        break;
                    case "1 Hour Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[5];
                        break;
                    case "2 Hours Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[6];
                        break;
                    case "1 Day Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[7];
                        break;
                    case "2 Days Before":
                        CurrentTask.ReminderTimeComboBoxItem = ReminderTimes[8];
                        break;
                }
            }
        }

        /// <summary>
        /// Schedules a toast notification using <see cref="ToastHelper"/> if the current task 
        /// has a selected due date, time due, reminder time when called.
        /// </summary>
        private void PrepareToastNotification()
        {
            // Note: UWP TimePicker doesn't support Nullable values, defaults to a value either way
            var dueDate = CurrentTask.DueDate.ToNullableDateTimeOffset();
            var timeDue = CurrentTask.TimeDue.ToNullableDateTimeOffset();
            var reminderTime = CurrentTask.ReminderTime;

            if (dueDate != null && timeDue != null && reminderTime != "None" && reminderTime != "")
            {
                // Combine due date and time due
                // ToastNotifications require a non-nullable DateTimeOffset
                DateTimeOffset taskDueDate = new DateTimeOffset(
                   dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                   timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                   timeDue.Value.Offset
                );

                var scheduledTime = taskDueDate;
                switch (reminderTime)
                {
                    case "At Time of Due Date":
                        break;
                    case "5 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-5);
                        break;
                    case "10 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-10);
                        break;
                    case "15 Minutes Before":
                        scheduledTime = taskDueDate.AddMinutes(-15);
                        break;
                    case "1 Hour Before":
                        scheduledTime = taskDueDate.AddHours(-1);
                        break;
                    case "2 Hours Before":
                        scheduledTime = taskDueDate.AddHours(-2);
                        break;
                    case "1 Day Before":
                        scheduledTime = taskDueDate.AddDays(-1);
                        break;
                    case "2 Days Before":
                        scheduledTime = taskDueDate.AddDays(-2);
                        break;
                }
                ToastHelper.ScheduleTaskDueNotification(CurrentTask.ID.ToString(), CurrentTask.Title,
                    CurrentTask.Description, scheduledTime, taskDueDate);
            }
            else if (reminderTime == "None")
                ToastHelper.RemoveScheduledNotification(CurrentTask.ID.ToString());
        }

        /// <summary>
        /// Sets the due date for the current task.
        /// </summary>
        /// <param name="dueDate"></param>
        public void SetDueDate(string dueDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.DueDate = dueDate;
        }

        /// <summary>
        /// Sets the start date for the current task.
        /// </summary>
        /// <param name="startDate"></param>
        public void SetStartDate(string startDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.StartDate = startDate;
        }

        /// <summary>
        /// Sets the finish date for the current task.
        /// </summary>
        /// <param name="finishDate"></param>
        public void SetFinishDate(string finishDate)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set due date.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.FinishDate = finishDate;

            // Determine new DaysWorkedOn value to update binding
            DateTimeOffset? startDate = CurrentTask.StartDate.ToNullableDateTimeOffset();
            if (finishDate != null && startDate != null)
            {
                TimeSpan? ts = finishDate.ToNullableDateTimeOffset() - startDate;

                if (ts != null)
                    // Difference in days, hous, mins
                    CurrentTask.DaysWorkedOn = String.Format("{0}d, {1}hrs, {2}min",
                        ts.Value.Days.ToString(), ts.Value.Hours.ToString(), ts.Value.Minutes.ToString());
            }
        }

        /// <summary>
        /// Sets the time due for the current task to be used for the toast notification.
        /// </summary>
        /// <param name="timeDue"></param>
        public void SetTimeDue(string timeDue)
        {
            if (CurrentTask == null)
                MessagePump.Show("Failed to set time due.  CurrentTask is null. Please try again or restart the application.", MessageDuration);

            CurrentTask.TimeDue = timeDue;
        }

        /// <summary>
        /// Inserts a tag to the current task's tag collection.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>A bool containing whether the tag was successfully added or not.</returns>
        public bool AddTag(string tag)
        {
            bool result = false;

            if (CurrentTask == null)
            {
                MessagePump.Show("Tag failed to be added.  CurrentTask is null. Please try again or restart the application.", MessageDuration);
                return result;
            }

            if (CurrentTask.Tags.Contains(tag))
                MessagePump.Show("Tag already exists", 3000);
            else
            {
                CurrentTask.Tags.Add(tag);
                if (!Board.TagsCollection.Contains(tag))
                    Board.TagsCollection.Add(tag);
                SuggestedTagsCollection.Remove(tag);
                MessagePump.Show($"Tag {tag} added successfully", 3000);
                result = true;
            }
            return result;
        }


        /// <summary>
        /// Updates the selected card category and column index after dragging it to
        /// a new column.
        /// </summary>
        /// <param name="targetCategory"></param>
        /// <param name="selectedCardModel"></param>
        /// <param name="targetIndex"></param>
        public void UpdateCardColumn(string targetCategory, PresentationTask selectedCardModel, int targetIndex)
        {
            TaskDTO task = selectedCardModel.To_TaskDTO();
            task.Category = targetCategory;
            task.ColumnIndex = targetIndex;
            DataProvider.Call(x => x.TaskServices.UpdateColumnData(task));
        }

        /// <summary>
        /// Updates a specific card index in the database when reordering after dragging a card.
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="currentCardIndex"></param>
        internal void UpdateCardIndex(int iD, int currentCardIndex)
        {
            DataProvider.Call(x => x.TaskServices.UpdateCardIndex(iD, currentCardIndex));
        }

        private ComboBoxItem GetComboBoxItemForColorKey(string colorKey) => ColorKeys.FirstOrDefault(x => x.Content.ToString() == colorKey);
        private ComboBoxItem GetComboBoxItemForReminderTime(string reminderTime) => ReminderTimes.FirstOrDefault(x => x.Content.ToString() == reminderTime);

        /// <summary>
        /// Shows a local notification on the current board using message as the content.
        /// </summary>
        /// <param name="message"></param>
        public void ShowInAppNotification(string message)
        {
            MessagePump.Show(message, MessageDuration);
        }

        #endregion
    }
}
