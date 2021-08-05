using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using LeaderAnalytics.AdaptiveClient;
using KanbanTasker.Base;
using KanbanTasker.Model;
using KanbanTasker.Models;
using KanbanTasker.Services;

namespace KanbanTasker.ViewModels
{
    public class MainViewModel : Observable
    {
        //private ObservableCollection<PresentationTask> allTasks;
        private const int MessageDuration = 3000;
        private readonly IAppNotificationService _appNotificationService;
        public Func<PresentationBoard, IAppNotificationService, BoardViewModel> boardViewModelFactory;
        private IAdaptiveClient<IServiceManifest> dataProvider;
        private ObservableCollection<BoardViewModel> _boardList;
        private BoardViewModel _currentBoard;
        private string _boardEditorTitle;
        // _tmpBoard is used to save the current board when a user clicks the Add button, than cancels.
        // Should be able to remove this property when this ticket is fixed: https://github.com/microsoft/microsoft-ui-xaml/issues/1200
        private BoardViewModel _tmpBoard;
        private string _oldBoardName;
        private string _oldBoardNotes;
       
        public ICommand NewBoardCommand { get; set; }
        public ICommand EditBoardCommand { get; set; }
        public ICommand SaveBoardCommand { get; set; }
        public ICommand CancelSaveBoardCommand { get; set; }
        public ICommand DeleteBoardCommand { get; set; }

        /// <summary>
        ///  Initializes boards and tasks.
        ///  Sorts the tasks by column index so that they are
        ///  loaded in as they were left when the app was last closed.
        /// </summary>
        public MainViewModel(
            Func<PresentationBoard, IAppNotificationService, BoardViewModel> boardViewModelFactory,
            IAdaptiveClient<IServiceManifest> dataProvider,
            Frame navigationFrame,
            IAppNotificationService appNotificationService)
        {
            this.NavigationFrame = navigationFrame;
            this._appNotificationService = appNotificationService;
            this.dataProvider = dataProvider;
            this.boardViewModelFactory = boardViewModelFactory;

            PropertyChanged += MainViewModel_PropertyChanged;
            NewBoardCommand = new RelayCommand(NewBoard, () => true);
            EditBoardCommand = new RelayCommand(EditBoard, () => CurrentBoard != null);
            SaveBoardCommand = new RelayCommand(SaveBoard, () => true);
            CancelSaveBoardCommand = new RelayCommand(CancelSaveBoard, () => true);
            DeleteBoardCommand = new RelayCommand(DeleteBoard, () => CurrentBoard != null);

            // Load Board Taskss
            BoardList = new ObservableCollection<BoardViewModel>();
            List<BoardDTO> boardDTOs = dataProvider.Call(x => x.BoardServices.GetBoards());

            foreach (BoardDTO dto in boardDTOs)
            {
                PresentationBoard presBoard = new PresentationBoard(dto);

                List<ColumnDTO> columnNames = dataProvider.Call(x => x.BoardServices.GetColumnNames(presBoard.ID));

                // Set column names


                if (dto.Tasks?.Any() ?? false)
                {
                    foreach (TaskDTO taskDTO in dto.Tasks.OrderBy(x => x.ColumnIndex))
                    {
                        presBoard.Tasks.Add(new PresentationTask(taskDTO));
                        
                        // Fill TagsCollection on Board for AutoSuggestBox
                        foreach (var tag in taskDTO.Tags.Split(','))
                            if (!string.IsNullOrEmpty(tag) && !presBoard.TagsCollection.Contains(tag))
                                presBoard.TagsCollection.Add(tag);
                    }
                }

                BoardList.Add(boardViewModelFactory(presBoard, appNotificationService));
            }

            CurrentBoard = BoardList.Any() ? BoardList.First() : null;
        }

        /// <summary>
        /// Notifies us of a property change when a user selects a board on the NavigationView in MainView.xaml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentBoard))
            {
                if (CurrentBoard == null)
                    NavigationFrame.Navigate(typeof(Views.NoBoardsMessageView));
                else
                    NavigationFrame.Navigate(typeof(Views.BoardView), CurrentBoard);
            }
        }

        private Frame NavigationFrame { get; set; }

        /// <summary>
        /// List of all boards
        /// </summary>
        public ObservableCollection<BoardViewModel> BoardList
        {
            get => _boardList;
            set => SetProperty(ref _boardList, value);
        }

        /// <summary>
        /// Currently selected board
        /// </summary>
        public BoardViewModel CurrentBoard
        {
            get => _currentBoard;
            set { _currentBoard = value; OnPropertyChanged(); }
        }

        public string BoardEditorTitle
        {
            get => _boardEditorTitle;
            set => SetProperty(ref _boardEditorTitle, value);
        }

        public string OldBoardName
        {
            get => _oldBoardName;
            set => SetProperty(ref _oldBoardName, value);
        }

        public string OldBoardNotes
        {
            get => _oldBoardNotes;
            set => SetProperty(ref _oldBoardNotes, value);
        }

        public void NewBoard()
        {
            BoardEditorTitle = "New Board Editor";
            BoardViewModel newBoard = boardViewModelFactory(new PresentationBoard(new BoardDTO()), _appNotificationService);
            _tmpBoard = CurrentBoard;            // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
            if (_tmpBoard != null)
            {
                OldBoardName = _tmpBoard.Board.Name;
                OldBoardNotes = _tmpBoard.Board.Notes;
            }
            CurrentBoard = null;                // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
            CurrentBoard = newBoard;
            // Don't add to BoardList here.  Wait till user saves.
        }

        public void EditBoard()
        {
            _tmpBoard = CurrentBoard;
            OldBoardName = _tmpBoard.Board.Name;
            OldBoardNotes = _tmpBoard.Board.Notes;
            BoardEditorTitle = "Edit Board";
        }

        public void SaveBoard()
        {
            if (CurrentBoard.Board == null)
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Name))
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Notes))
                return;

            // Database validation will handle missing values and display an error message if necessary
            //if (string.IsNullOrEmpty(CurrentBoard.Board.Name))
            //    return;
            //if (string.IsNullOrEmpty(CurrentBoard.Board.Notes))
            //    return;

            BoardDTO dto = CurrentBoard.Board.To_BoardDTO();
            bool isNew = dto.Id == 0;
            RowOpResult<BoardDTO> result = null;
            // Add board to db and collection
            result = dataProvider.Call(x => x.BoardServices.SaveBoard(dto));
            _appNotificationService.DisplayNotificationAsync(result.Success ? "Board was saved successfully." : result.ErrorMessage, MessageDuration);
            if (isNew && result.Success)
            {
                CurrentBoard.Board.ID = result.Entity.Id;
                BoardList.Add(CurrentBoard);
            }

        }

        public void CancelSaveBoard()
        {
            // BUG: Currently _tmpBoard still holds edited version?
            CurrentBoard.Board.Name = "";
            CurrentBoard.Board.Notes = "";
            CurrentBoard = null; 
            CurrentBoard = _tmpBoard;

            // hack
            if (CurrentBoard != null)
            {
                CurrentBoard.Board.Name = OldBoardName;
                CurrentBoard.Board.Notes = OldBoardNotes;
            }
        }

        public void DeleteBoard()
        {
            if (CurrentBoard == null)
                return;

            dataProvider.Call(x => x.BoardServices.DeleteBoard(CurrentBoard.Board.ID));
            BoardList.Remove(CurrentBoard);
            CurrentBoard.Board.Name = ""; // uwp bug
            CurrentBoard.Board.Notes = ""; // uwp bug

            CurrentBoard = null; // uwp bug
            CurrentBoard = BoardList.LastOrDefault();
        }

        internal void SetCurrentBoardOnClose()
        {
            if (BoardList.Count == 0)
                CurrentBoard = null; // Displays NoBoardsView after
            else
            {
                CurrentBoard = null;
                CurrentBoard = _tmpBoard;
            }
        }
    }
}