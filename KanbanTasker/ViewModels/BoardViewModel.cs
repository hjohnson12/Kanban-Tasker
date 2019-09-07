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
        
        private string _paneTitle;
        private bool _isPointerEntered = false;
        private bool _isEditingTask;
        private string _currentCategory;
        private IKanbanTaskerService DataProvider;
        public ICommand NewTaskCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand SaveTaskCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }
        public ICommand DeleteTagCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }

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
        private const int MessageDuration = 3000;

        #endregion Properties

        /// <summary>
        /// Constructor / Initialization of tasks
        /// </summary>
        public BoardViewModel(PresentationBoard board, IKanbanTaskerService dataProvider, InAppNotification messagePump)
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

            ColorKeys = new ObservableCollection<ComboBoxItem>();
            ColorKeys.Add(new ComboBoxItem { Content = "High" });
            ColorKeys.Add(new ComboBoxItem { Content = "Normal" });
            ColorKeys.Add(new ComboBoxItem { Content = "Low" });

            if (Board.Tasks != null && board.Tasks.Any())   // hack
                foreach (PresentationTask task in Board.Tasks)
                    task.ColorKeyComboBoxItem = GetComboBoxItemForColorKey(task.ColorKey);
        }

        #region Functions


        public void NewTaskCommandHandler(ColumnTag tag)
        {
            PaneTitle = "New Task";
            string category = tag?.Header?.ToString();
            CurrentTask = new PresentationTask(new TaskDTO() { Category = category }) { Board = Board, BoardId = Board.ID,  ColorKeyComboBoxItem = ColorKeys[1] };
            OriginalTask = null; 
            IsEditingTask = true;
        }

        public void EditTaskCommandHandler(int taskID)
        {
            PaneTitle = "Edit Task";
            CurrentTask = Board.Tasks.First(x => x.ID == taskID);
            IsEditingTask = true;
            // clone a copy of CurrentTask so we can restore if user cancels
            OriginalTask = new PresentationTask(CurrentTask.To_TaskDTO());
        }

        public void SaveTaskCommandHandler()
        {
            IsEditingTask = false;

            if (CurrentTask == null)
                return;

            TaskDTO dto = CurrentTask.To_TaskDTO();
            dto.ColorKey = ((ComboBoxItem)CurrentTask.ColorKeyComboBoxItem)?.Content.ToString() ?? "Normal"; // hack
            
            bool isNew = dto.Id == 0;

            if (isNew)
            {
                dto.ColumnIndex = Board.Tasks?.Where(x => x.Category == dto.Category).Count() ?? 0;
                dto.DateCreated = DateTime.Now.ToString();
                dto.Id = DataProvider.AddTask(dto).Entity.Id;
            }
            else
                DataProvider.UpdateTask(dto);

            if (isNew)
            {
                CurrentTask.ID = dto.Id;
                CurrentTask.ColumnIndex = dto.ColumnIndex;
                Board.Tasks.Add(CurrentTask);
            }
            
            MessagePump.Show("Task was saved successfully", MessageDuration);
        }

        public void DeleteTaskCommandHandler(int taskID)
        {
            PresentationTask task = Board.Tasks.First(x => x.ID == taskID);
            bool success = DataProvider.DeleteTask(taskID);

            if (success)
            {
                Board.Tasks.Remove(task);
                CurrentTask = Board.Tasks.LastOrDefault();
                int startIndex = task.ColumnIndex.Value;

                foreach (PresentationTask otherTask in Board.Tasks.Where(x => x.Category == task.Category && x.ColumnIndex > task.ColumnIndex))
                {
                    otherTask.ColumnIndex = startIndex++;
                    UpdateCardIndex(otherTask.ID, otherTask.ColumnIndex.Value);
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
            }
        }

        public bool AddTag(string tag)
        {
            bool result = false;

            if (CurrentTask == null)
            {
                MessagePump.Show("Tag failed to be deleted.  CurrentTask is null. Please try again or restart the application.", MessageDuration);
                return result;
            }

            if (CurrentTask.Tags.Contains(tag))
                MessagePump.Show("Tag already exists", 3000);
            else
            {
                CurrentTask.Tags.Add(tag);
                MessagePump.Show($"Tag {tag} added successfully", 3000);
                result = true;
            }
            return result;
        }


        /// <summary>
        /// Updates the selected card category and column index after dragging it to
        /// a new column
        /// </summary>
        /// <param name="targetCategory"></param>
        /// <param name="selectedCardModel"></param>
        /// <param name="targetIndex"></param>
        public void UpdateCardColumn(string targetCategory, PresentationTask selectedCardModel, int targetIndex)
        {
            TaskDTO task = selectedCardModel.To_TaskDTO();
            task.Category = targetCategory;
            task.ColumnIndex = targetIndex;
            DataProvider.UpdateColumnData(task);
        }

        /// <summary>
        /// Updates a specific card index in the database when reordering after dragging a card
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="currentCardIndex"></param>
        internal void UpdateCardIndex(int iD, int currentCardIndex)
        {
            DataProvider.UpdateCardIndex(iD, currentCardIndex);
        }

        private ComboBoxItem GetComboBoxItemForColorKey(string colorKey) => ColorKeys.FirstOrDefault(x => x.Content.ToString() == colorKey);

        #endregion
    }
}
