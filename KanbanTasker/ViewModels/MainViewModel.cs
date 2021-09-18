using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LeaderAnalytics.AdaptiveClient;
using KanbanTasker.Base;
using KanbanTasker.Model;
using KanbanTasker.Models;
using KanbanTasker.Services;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using KanbanTasker.Model.Services;

namespace KanbanTasker.ViewModels
{
    public class MainViewModel : Observable
    {
        private const int MessageDuration = 3000;
        private readonly IAppNotificationService _appNotificationService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IAdaptiveClient<IServiceManifest> dataProvider;
        public Func<PresentationBoard, IAppNotificationService, BoardViewModel> boardViewModelFactory;
        private ObservableCollection<BoardViewModel> _boardList;
        private BoardViewModel _currentBoard;
        // _tmpBoard is used to save the current board when a user clicks the Add button, than cancels.
        // Should be able to remove this property when this ticket is fixed: https://github.com/microsoft/microsoft-ui-xaml/issues/1200
        private BoardViewModel _tmpBoard;
        private string _boardEditorTitle;
        private string _oldBoardName;
        private string _oldBoardNotes;

        public ICommand NewBoardCommand { get; set; }
        public ICommand EditBoardCommand { get; set; }
        public ICommand SaveBoardCommand { get; set; }
        public ICommand CancelSaveBoardCommand { get; set; }
        public ICommand DeleteBoardCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenCalendarCommand { get; set; }

        /// <summary>
        ///  Initializes boards and tasks.
        ///  Sorts the tasks by column index so that they are
        ///  loaded in as they were left when the app was last closed.
        /// </summary>
        public MainViewModel(
            Func<PresentationBoard, IAppNotificationService, BoardViewModel> boardViewModelFactory,
            IAdaptiveClient<IServiceManifest> dataProvider,
            INavigationService navigationService,
            IAppNotificationService appNotificationService,
            IDialogService dialogService)
        {
            this.boardViewModelFactory = boardViewModelFactory;
            this.dataProvider = dataProvider;
            this._navigationService = navigationService;
            this._appNotificationService = appNotificationService;
            this._dialogService = dialogService;

            PropertyChanged += MainViewModel_PropertyChanged;
            NewBoardCommand = new Base.RelayCommand(NewBoard, () => true);
            EditBoardCommand = new Base.RelayCommand(EditBoard, () => CurrentBoard != null);
            SaveBoardCommand = new Base.RelayCommand(SaveBoard, () => true);
            CancelSaveBoardCommand = new Base.RelayCommand(CancelSaveBoard, () => true);
            DeleteBoardCommand = new Base.RelayCommand(DeleteBoard, () => CurrentBoard != null);
            OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsDialog, () => true);
            OpenCalendarCommand = new AsyncRelayCommand(OpenCalendarDialog, () => true);

            InitializeBoards();
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
                    _navigationService.NavigateToDefaultView();
                else
                    _navigationService.NavigateToBoard(CurrentBoard);
            }
        }

        /// <summary>
        /// Initializes each board, if any exist, with tasks and sets the current board
        /// to the first.
        /// </summary>
        private void InitializeBoards()
        {
            BoardList = new ObservableCollection<BoardViewModel>();
            List<BoardDTO> boardDTOs = dataProvider.Call(x => x.BoardServices.GetBoards());

            foreach (BoardDTO dto in boardDTOs)
            {
                PresentationBoard presBoard = new PresentationBoard(dto);

                // Fill board with tasks
                if (dto.Tasks?.Any() ?? false)
                {
                    foreach (TaskDTO taskDTO in dto.Tasks.OrderBy(x => x.ColumnIndex))
                    {
                        presBoard.Tasks.Add(new PresentationTask(taskDTO));

                        // Fill TagsCollection on Board for AutoSuggestBox
                        foreach (var tag in taskDTO.Tags.Split(','))
                        {
                            if (!string.IsNullOrEmpty(tag) && !presBoard.TagsCollection.Contains(tag))
                                presBoard.TagsCollection.Add(tag);
                        }
                    }
                }
                BoardList.Add(boardViewModelFactory(presBoard, _appNotificationService));
            }
            CurrentBoard = BoardList.Any() ? BoardList.First() : null;
        }

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

        public async void NewBoard()
        {
            var dialogOpen = _dialogService.CheckForOpenDialog();
            if (!dialogOpen)
            {
                BoardEditorTitle = "New Board Editor";

                BoardViewModel newBoard = boardViewModelFactory(
                    new PresentationBoard(new BoardDTO()),
                    _appNotificationService);

                _tmpBoard = CurrentBoard;            // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
                if (_tmpBoard != null)
                {
                    _oldBoardName = _tmpBoard.Board.Name;
                    _oldBoardNotes = _tmpBoard.Board.Notes;
                }

                CurrentBoard = null;                // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
                CurrentBoard = newBoard;
                // Don't add to BoardList here.  Wait till user saves.

                await _dialogService.ShowEditBoardDialog(this);
            }
        }

        public async void EditBoard()
        {
            BoardEditorTitle = "Edit Board";

            _tmpBoard = CurrentBoard;
            _oldBoardName = _tmpBoard.Board.Name;
            _oldBoardNotes = _tmpBoard.Board.Notes;

            await _dialogService.ShowEditBoardDialog(this);
        }

        public void SaveBoard()
        {
            if (CurrentBoard.Board == null)
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Name))
                return;
            if (string.IsNullOrEmpty(CurrentBoard.Board.Notes))
                return;

            BoardDTO dto = CurrentBoard.Board.To_BoardDTO();
            bool isNew = dto.Id == 0;
            RowOpResult<BoardDTO> result = null;

            // Add board to db and collection
            result = dataProvider.Call(x => x.BoardServices.SaveBoard(dto));

            if (isNew && result.Success)
            {
                CurrentBoard.Board.ID = result.Entity.Id;
                BoardList.Add(CurrentBoard);
                dataProvider.Call(x => x.BoardServices.CreateColumns(CurrentBoard.Board.ID));

                // Set columns to collection
                CurrentBoard.BoardColumns.Clear();
                var columns = dataProvider.Call(x => x.BoardServices.GetColumns(CurrentBoard.Board.ID));
                for (int i = 0; i < columns.Count; i++)
                {
                    CurrentBoard.BoardColumns.Add(new PresentationBoardColumn(
                        columns.Find(x => x.Position == i)));
                }
            }

            _appNotificationService.DisplayNotificationAsync(
                result.Success ? "Board was saved successfully." : result.ErrorMessage,
                MessageDuration);
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
                CurrentBoard.Board.Name = _oldBoardName;
                CurrentBoard.Board.Notes = _oldBoardNotes;
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

        public async Task OpenSettingsDialog()
        {
            await _dialogService.ShowSettingsDialog();
        }

        public async Task OpenCalendarDialog()
        {
            if (CurrentBoard != null)
                await _dialogService.ShowCalendarDialog(this);
        }
    }
}