using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Syncfusion.UI.Xaml.Kanban;
using LeaderAnalytics.AdaptiveClient;
using KanbanTasker.Base;
using KanbanTasker.Model;
using KanbanTasker.Extensions;
using KanbanTasker.Helpers;
using KanbanTasker.Models;
using KanbanTasker.Services;

namespace KanbanTasker.ViewModels
{
    public class BoardViewModel : Observable
    {
        private const int NOTIFICATION_DURATION = 3000;
        private const string DEFAULT_COLOR_KEY = "Low";
        private const string DEFAULT_REMINDER_TIME = "None";
        private readonly IAppNotificationService _appNotificationService;
        private IAdaptiveClient<IServiceManifest> DataProvider;
        private PresentationBoard _board;
        private PresentationTask _currentTask;
        private ObservableCollection<string> _suggestedTagsCollection;
        private Brush _dueDateBackgroundBrush;
        private string _paneTitle;
        private string _currentCategory;
        private bool _isPointerEntered = false;
        private bool _isEditingTask;
        private bool _isProgressRingActive = false;
        private DispatcherTimer _dateCheckTimer;
        private ObservableCollection<string> _colorKeys;
        private ObservableCollection<string> _reminderTimes;

        public ICommand NewTaskCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand SaveTaskCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }
        public ICommand DeleteTagCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }
        public ICommand RemoveScheduledNotificationCommand { get; set; }

        /// <summary>
        /// Initializes the commands and tasks for the current board.
        /// </summary>
        public BoardViewModel(
            PresentationBoard board,
            IAdaptiveClient<IServiceManifest> dataProvider,
            IAppNotificationService appNotificationService)
        {
            Board = board;
            DataProvider = dataProvider;
            _appNotificationService = appNotificationService;

            CurrentTask = new PresentationTask(new TaskDTO());
            NewTaskCommand = new RelayCommand<ColumnTag>(NewTask, () => true);
            EditTaskCommand = new RelayCommand<int>(EditTask, () => true);
            SaveTaskCommand = new RelayCommand(SaveTask, () => true);
            DeleteTaskCommand = new RelayCommand<int>(DeleteTask, () => PaneTitle.Equals("Edit Task") || PaneTitle.Equals(""));
            DeleteTagCommand = new RelayCommand<string>(DeleteTag, () => true);
            CancelEditCommand = new RelayCommand(CancelEdit, () => true);
            RemoveScheduledNotificationCommand = new RelayCommand(RemoveScheduledNotfication, () => true);

            DueDateBackgroundBrush = Application.Current.Resources["RegionBrush"] as AcrylicBrush;

            ColorKeys = new ObservableCollection<string>
            {
                "Low", "Medium", "High"
            };

            ReminderTimes = new ObservableCollection<string>
            {
                "None",
                "At Time of Due Date",
                "5 Minutes Before",
                "10 Minutes Before",
                "15 Mintues Before",
                "1 Hour Before",
                "2 Hours Before",
                "1 Day Before",
                "2 Days Before"
            };

            PaneTitle = "New Task";
        }

        /// <summary>
        /// An instance of the current board
        /// </summary>
        public PresentationBoard Board
        {
            get => _board;
            set => SetProperty(ref _board, value);
        }

        /// <summary>
        /// An instance of the current task
        /// </summary>
        public PresentationTask CurrentTask
        {
            get => _currentTask;
            set => SetProperty(ref _currentTask, value);
        }

        public ObservableCollection<string> SuggestedTagsCollection
        {
            get => _suggestedTagsCollection;
            set => SetProperty(ref _suggestedTagsCollection, value);
        }

        public Brush DueDateBackgroundBrush
        {
            get => _dueDateBackgroundBrush;
            set => SetProperty(ref _dueDateBackgroundBrush, value);
        }

        /// <summary>
        /// Flag for enabling a progress ring when true
        /// </summary>
        public bool IsProgressRingActive
        {
            get => _isProgressRingActive;
            set => SetProperty(ref _isProgressRingActive, value);
        }

        /// <summary>
        /// The color keys for a task
        /// </summary>
        public ObservableCollection<string> ColorKeys
        {
            get => _colorKeys;
            set => SetProperty(ref _colorKeys, value);
        }

        /// <summary>
        /// The possible reminder times for a task 
        /// </summary>
        public ObservableCollection<string> ReminderTimes
        {
            get => _reminderTimes;
            set => SetProperty(ref _reminderTimes, value);
        }

        /// <summary>
        /// The name of the editing pane
        /// </summary>
        public string PaneTitle
        {
            get => _paneTitle;
            set => SetProperty(ref _paneTitle, value);
        }

        /// <summary>
        /// Determines if pointer entered
        /// inside of a card
        /// </summary>
        public bool IsPointerEntered
        {
            get => _isPointerEntered;
            set => SetProperty(ref _isPointerEntered, value);
        }

        /// <summary>
        /// Flag for if a task is currently being edited
        /// </summary>
        public bool IsEditingTask
        {
            get => _isEditingTask;
            set => SetProperty(ref _isEditingTask, value);
        }

        /// <summary>
        /// The category to be displayed on the edit/new task pane
        /// </summary>
        public string CurrentCategory
        {
            get => _currentCategory;
            set => SetProperty(ref _currentCategory, value);
        }

        /// <summary>
        /// Gets or sets the selected item of the color key combo box. 
        /// Updates the current tasks color key.
        /// </summary>
        public string SelectedColorKey
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentTask.ColorKey))
                    return "Low";
                else
                    return CurrentTask.ColorKey;
            }
            set
            {
                CurrentTask.ColorKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected item of the reminder time combo box.
        /// Updates the current tasks reminder time.
        /// </summary>
        public string SelectedReminderTime
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentTask.ReminderTime))
                    return "None";
                else
                    return CurrentTask.ReminderTime;
            }
            set
            {
                CurrentTask.ReminderTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Used to make a copy of the current task to revert changes when cancelling editing
        /// </summary>
        public PresentationTask OriginalTask { get; set; }

        public void NewTask(ColumnTag tag)
        {
            IsEditingTask = true;
            PaneTitle = "New Task";
            string category = tag?.Header?.ToString();

            CurrentTask = new PresentationTask(new TaskDTO() { Category = category })
            {
                Board = Board,
                BoardId = Board.ID
            };

            SelectedColorKey = DEFAULT_COLOR_KEY;
            SelectedReminderTime = DEFAULT_REMINDER_TIME;
            OriginalTask = null; 
            InitializeSuggestedTags();
        }

        public void EditTask(int taskID)
        {
            IsEditingTask = true;
            PaneTitle = "Edit Task";
            CurrentTask = Board.Tasks.First(x => x.ID == taskID);
            SelectedColorKey = CurrentTask.ColorKey;
            SelectedReminderTime = CurrentTask.ReminderTime;
            InitializeSuggestedTags();
            InitializeDateInformation();
            // clone a copy of CurrentTask so we can restore if user cancels
            OriginalTask = new PresentationTask(CurrentTask.To_TaskDTO());
        }

        public void SaveTask()
        {
            IsEditingTask = false;
            PaneTitle = "";

            if (_dateCheckTimer != null)
                _dateCheckTimer.Stop();

            if (CurrentTask == null)
                return;

            TaskDTO taskDTO = CurrentTask.To_TaskDTO();

            bool isNew = taskDTO.Id == 0;
            if (isNew)
            {
                taskDTO.ColumnIndex = Board.Tasks?.Where(x => x.Category == taskDTO.Category).Count() ?? 0;
                taskDTO.DateCreated = DateTime.Now.ToString();
            }
            taskDTO.Id = DataProvider.Call(x => x.TaskServices.SaveTask(taskDTO)).Entity.Id;

            if (isNew)
            {
                CurrentTask.ID = taskDTO.Id;
                CurrentTask.ColumnIndex = taskDTO.ColumnIndex;
                Board.Tasks.Add(CurrentTask);
            }

            if (IsReminderInformationNull())
            {
                PrepareAndScheduleToastNotification();
            }

            _appNotificationService.DisplayNotificationAsync("Task was saved successfully", NOTIFICATION_DURATION);
        }

        public bool IsReminderInformationNull()
        {
            return CurrentTask.DueDate != null &&
                CurrentTask.TimeDue != null &&
                CurrentTask.ReminderTime != "None" &&
                CurrentTask.ReminderTime != "";
        }

        public void DeleteTask(int taskID)
        {
            if (_dateCheckTimer != null)
            {
                _dateCheckTimer.Stop();
            }

            PresentationTask task = Board.Tasks.First(x => x.ID == taskID);
            RowOpResult result = DataProvider.Call(x => x.TaskServices.DeleteTask(taskID));

            if (result.Success)
            {
                Board.Tasks.Remove(task);
                CurrentTask = Board.Tasks.LastOrDefault();
                ToastNotificationHelper.RemoveScheduledNotification(taskID.ToString());
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
                _appNotificationService.DisplayNotificationAsync("Task deleted from board successfully", NOTIFICATION_DURATION);
            }
            else
            {
                _appNotificationService.DisplayNotificationAsync("Task failed to be deleted. Please try again or restart the application.", NOTIFICATION_DURATION);
            }
        }

        public void DeleteTag(string tag)
        {
            if (CurrentTask == null)
            {
                _appNotificationService.DisplayNotificationAsync(
                    "Tag failed to be deleted. CurrentTask is null. Please try again or restart the application.",
                    NOTIFICATION_DURATION);
                return;
            }
            CurrentTask.Tags.Remove(tag);

            _appNotificationService.DisplayNotificationAsync("Tag deleted successfully", NOTIFICATION_DURATION);
        }

        public void CancelEdit()
        {
            IsProgressRingActive = true;
            IsEditingTask = false;
            PaneTitle = "";

            if (_dateCheckTimer != null)
                _dateCheckTimer.Stop();

            // Roll back changes to Current Task
            // when the original task exists
            if (OriginalTask != null)
            {
                int index = Board.Tasks.IndexOf(CurrentTask);
                var tempTask = new PresentationTask(OriginalTask.To_TaskDTO());
                Board.Tasks[index] = tempTask;
                Board.Tasks = new ObservableCollection<PresentationTask>(
                    Board.Tasks.OrderBy(x => x.ColumnIndex));

                // Check if a toast notification was deleted
                if (OriginalTask.ReminderTime != DEFAULT_REMINDER_TIME)
                    PrepareAndScheduleToastNotification();
            }

            IsProgressRingActive = false;
        }

        /// <summary>
        /// Removes the scheuled notification for the current task, 
        /// uniqely identified by its tag. <br />
        /// In this case, the tag is the task's unique ID.
        /// </summary>
        private void RemoveScheduledNotfication()
        {
            ToastNotificationHelper.RemoveScheduledNotification(CurrentTask.ID.ToString());
            CurrentTask.ReminderTime = DEFAULT_REMINDER_TIME;
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
                _appNotificationService.DisplayNotificationAsync("Tag failed to be added.  CurrentTask is null. Please try again or restart the application.", NOTIFICATION_DURATION);
                return result;
            }

            if (CurrentTask.Tags.Contains(tag))
                _appNotificationService.DisplayNotificationAsync("Tag already exists", 3000);
            else
            {
                CurrentTask.Tags.Add(tag);
                if (!Board.TagsCollection.Contains(tag))
                    Board.TagsCollection.Add(tag);
                SuggestedTagsCollection.Remove(tag);
                _appNotificationService.DisplayNotificationAsync($"Tag {tag} added successfully", 3000);
                result = true;
            }
            return result;
        }

        private void InitializeSuggestedTags()
        {
            // Removes tags from suggested list that are already on the tag, if any
            SuggestedTagsCollection = Board.TagsCollection;
            foreach (var tag in CurrentTask.Tags)
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
            if (IsEditingTask)
            {
                StartDateCheckTimer();
                UpdateDateInformation();
            }
        }

        /// <summary>
        /// Schedules a toast notification using <see cref="ToastNotificationHelper"/> if the current task 
        /// has a selected due date, time due, reminder time when called.
        /// </summary>
        private void PrepareAndScheduleToastNotification()
        {
            // UWP TimePicker doesn't support Nullable values, defaults to a value either way
            var dueDate = CurrentTask.DueDate.ToNullableDateTimeOffset();
            var timeDue = CurrentTask.TimeDue.ToNullableDateTimeOffset();
            string reminderTime = CurrentTask.ReminderTime;

            if (reminderTime.Equals(DEFAULT_REMINDER_TIME))
                ToastNotificationHelper.RemoveScheduledNotification(CurrentTask.ID.ToString());
            else
            {
                // Combine due date and time due
                // ToastNotifications require a non-nullable DateTimeOffset
                var taskDueDate = new DateTimeOffset(
                   dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                   timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                   timeDue.Value.Offset
                );

                DateTimeOffset scheduledTime = taskDueDate;
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

                ToastNotificationHelper.ScheduleTaskDueNotification(
                    CurrentTask.ID.ToString(),
                    CurrentTask.Title,
                    CurrentTask.Description,
                    scheduledTime,
                    taskDueDate);
            }
        }

        private void StartDateCheckTimer()
        {
            _dateCheckTimer = new DispatcherTimer();
            _dateCheckTimer.Interval = TimeSpan.FromMinutes(1);
            _dateCheckTimer.Tick += Timer_Tick;
            _dateCheckTimer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateDateInformation();
        }

        private void UpdateDateInformation()
        {
            DateTimeOffset? dateCreated = CurrentTask.DateCreated.ToNullableDateTimeOffset();
            DateTimeOffset? today = DateTimeOffset.Now;
            DateTimeOffset? startDate = CurrentTask.StartDate.ToNullableDateTimeOffset();
            DateTimeOffset? finishDate = CurrentTask.FinishDate.ToNullableDateTimeOffset();

            CheckIfPassedDueDate(); // Sets background of CalendarPicker red if past due

            // Update DaysWorkedOn
            if (startDate != null)
            {
                TimeSpan? ts = today - startDate;

                if (ts != null)
                    // Difference in days, hous, mins
                    CurrentTask.DaysWorkedOn = string.Format("{0} day(s)",
                        ts.Value.Days.ToString());
            }

            //  Update DaysSinceCreation
            if (dateCreated != null && today != null)
            {
                TimeSpan? timeSpan = today - dateCreated;

                if (timeSpan != null)
                {
                    // Difference in days, hours, mins
                    CurrentTask.DaysSinceCreation = string.Format("{0}d, {1}hrs, {2}min",
                        timeSpan.Value.Days.ToString(),
                        timeSpan.Value.Hours.ToString(),
                        timeSpan.Value.Minutes.ToString());
                }
            }
        }

        /// <summary>
        /// Sets the due date for the current task.
        /// </summary>
        /// <param name="dueDate"></param>
        public void SetDueDate(string dueDate)
        {
            if (CurrentTask == null)
            {
                _appNotificationService.DisplayNotificationAsync(
                    "Failed to set due date.  CurrentTask is null. Please try again or restart the application.",
                    NOTIFICATION_DURATION);
            }

            CurrentTask.DueDate = dueDate;
            CheckIfPassedDueDate();
        }

        /// <summary>
        /// Sets the start date for the current task.
        /// </summary>
        /// <param name="startDate"></param>
        public void SetStartDate(string startDate)
        {
            if (CurrentTask == null)
            {
                _appNotificationService.DisplayNotificationAsync(
                    "Failed to set due date.  CurrentTask is null. Please try again or restart the application.",
                    NOTIFICATION_DURATION);
            }

            CurrentTask.StartDate = startDate;

            // Update DaysWorkedOn binding
            DateTimeOffset? today = DateTimeOffset.Now;
            if (startDate.ToNullableDateTimeOffset() != null)
            {
                TimeSpan? ts = today - startDate.ToNullableDateTimeOffset();

                if (ts != null)
                {
                    // Difference in days, hous, mins
                    CurrentTask.DaysWorkedOn = String.Format("{0} day(s)",
                        ts.Value.Days.ToString());
                }
            }
        }

        /// <summary>
        /// Sets the finish date for the current task.
        /// </summary>
        /// <param name="finishDate"></param>
        public void SetFinishDate(string finishDate)
        {
            if (CurrentTask == null)
            {
                _appNotificationService.DisplayNotificationAsync(
                    "Failed to set due date.  CurrentTask is null. Please try again or restart the application.",
                    NOTIFICATION_DURATION);
            }

            CurrentTask.FinishDate = finishDate;

            // Determine new DaysWorkedOn value to update binding
            //DateTimeOffset? startDate = CurrentTask.StartDate.ToNullableDateTimeOffset();
            //if (finishDate != null && startDate != null)
            //{
            //    TimeSpan? ts = finishDate.ToNullableDateTimeOffset() - startDate;

            //    if (ts != null)
            //        // Difference in days, hous, mins
            //        CurrentTask.DaysWorkedOn = String.Format("{0}d, {1}hrs, {2}min",
            //            ts.Value.Days.ToString(), ts.Value.Hours.ToString(), ts.Value.Minutes.ToString());
            //}
        }

        /// <summary>
        /// Sets the time due for the current task to be used for the toast notification.
        /// </summary>
        /// <param name="timeDue"></param>
        public void SetTimeDue(string timeDue)
        {
            if (CurrentTask == null)
            {
                _appNotificationService.DisplayNotificationAsync(
                    "Failed to set time due.  CurrentTask is null. Please try again or restart the application.",
                    NOTIFICATION_DURATION);
            }

            CurrentTask.TimeDue = timeDue;
            CheckIfPassedDueDate();
        }

        /// <summary>
        /// Checks to see if the current task's due date has already passed and sets the
        /// due date background, respectively. <br />
        /// If true, sets the background of the CalendarPicker red. <br /> 
        /// Otherwise, sets the background to the default brush.
        /// <para>Note: If no due date has been selected, no changes
        /// will be made since the current task's date is null. </para>
        /// </summary>
        public void CheckIfPassedDueDate()
        {
            var dueDate = CurrentTask.DueDate.ToNullableDateTimeOffset();
            if (!(dueDate == null))
            {
                var timeDue = CurrentTask.TimeDue.ToNullableDateTimeOffset();
                DateTimeOffset today = DateTimeOffset.Now;

                DateTimeOffset taskDueDate = new DateTimeOffset(
                  dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                  timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                  timeDue.Value.Offset
                );

                if (DateTimeOffset.Compare(taskDueDate, today) < 0)
                    DueDateBackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Red) { Opacity = 0.6 };
                else
                    DueDateBackgroundBrush = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
            }
            else
                DueDateBackgroundBrush = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
        }

        /// <summary>
        /// Shows a local notification on the current board using message as the content.
        /// </summary>
        /// <param name="message"></param>
        public void ShowInAppNotification(string message)
        {
            _appNotificationService.DisplayNotificationAsync(message, NOTIFICATION_DURATION);
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
    }
}