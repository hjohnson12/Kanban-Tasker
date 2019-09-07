using KanbanTasker.Base;
using KanbanTasker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KanbanTasker.Services;
using KanbanTasker.Model;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace KanbanTasker.ViewModels
{
    public class MainViewModel : Observable
    {
        //private ObservableCollection<PresentationTask> allTasks;
        public Func<PresentationBoard, InAppNotification, BoardViewModel> boardViewModelFactory;
        private IKanbanTaskerService dataProvider;
        public ICommand NewBoardCommand { get; set; }
        public ICommand EditBoardCommand { get; set; }
        public ICommand SaveBoardCommand { get; set; }
        public ICommand CancelSaveBoardCommand { get; set; }
        public ICommand DeleteBoardCommand { get; set; }

        #region Properties

        /// <summary>
        /// List of all boards
        /// </summary>
        private ObservableCollection<BoardViewModel> _boardList;
        public ObservableCollection<BoardViewModel> BoardList
        {
            get
            {
                return _boardList;
            }
            set
            {
                _boardList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Currently selected board
        /// </summary>
        private BoardViewModel _CurrentBoard;
        public BoardViewModel CurrentBoard
        {
            get
            {
                return _CurrentBoard;
            }
            set
            {
                _CurrentBoard = value;
                OnPropertyChanged();
            }

        }
        private string _BoardEditorTitle;
        public string BoardEditorTitle
        {
            get => _BoardEditorTitle;
            set
            {
                _BoardEditorTitle = value;
                OnPropertyChanged();
            }
        }

        internal void SetCurrentBoardOnClose()
        {
            if (CurrentBoard.Board.Name == "" || CurrentBoard.Board.Notes == "")
                CurrentBoard = null;
            else
            {
                CurrentBoard = null;
                CurrentBoard = TmpBoard;
            }
        }

        private Frame navigationFrame { get; set; }
        private InAppNotification messagePump;
        private const int MessageDuration = 3000;

        // TmpBoard is used to save the current board when a user clicks the Add button, than cancels.  Should be able to remove this property when this ticket is fixed: https://github.com/microsoft/microsoft-ui-xaml/issues/1200
        private BoardViewModel TmpBoard; 
        #endregion Properties


        /// <summary>
        ///  Constructor / Initiliazation of boards and tasks.
        ///  Sorts the tasks by column index so that they are
        ///  loaded in as they were left when the app closed
        /// </summary>
        public MainViewModel(Func<PresentationBoard, InAppNotification, BoardViewModel> boardViewModelFactory, IKanbanTaskerService dataProvider, Frame navigationFrame, InAppNotification messagePump)
        {
            this.navigationFrame = navigationFrame;
            this.messagePump = messagePump;
            PropertyChanged += MainViewModel_PropertyChanged;
            NewBoardCommand = new RelayCommand(NewBoardCommandHandler, () => true);
            EditBoardCommand = new RelayCommand(EditBoardCommandHandler, () => CurrentBoard != null);
            SaveBoardCommand = new RelayCommand(SaveBoardCommandHandler, () => true);
            CancelSaveBoardCommand = new RelayCommand(CancelSaveBoardCommandHandler, () => true);
            DeleteBoardCommand = new RelayCommand(DeleteBoardCommandHandler, () => CurrentBoard != null);
            this.dataProvider = dataProvider;
            this.boardViewModelFactory = boardViewModelFactory;
            BoardList = new ObservableCollection<BoardViewModel>();
            List<BoardDTO> boardDTOs = dataProvider.GetBoards();

            foreach (BoardDTO dto in boardDTOs)
                BoardList.Add(boardViewModelFactory(new PresentationBoard(dto), messagePump));

            if (BoardList.Any())
                CurrentBoard = BoardList.First();
            else
                CurrentBoard = null;
        }

        // We need to know when user selects a board on the NavigationView in MainView.xaml.
        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentBoard))
            {
                if (CurrentBoard == null)
                    navigationFrame.Navigate(typeof(Views.NoBoardsMessageView));
                else
                    navigationFrame.Navigate(typeof(Views.BoardView), CurrentBoard);
            }
        }

     
        public void NewBoardCommandHandler()
        {
            BoardEditorTitle = "New Board Editor";
            BoardViewModel newBoard = boardViewModelFactory(new PresentationBoard(new BoardDTO()), messagePump);
            TmpBoard = CurrentBoard;            // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
            CurrentBoard = null;                // Workaround for this issue.  Don't remove this line till it's fixed. https://github.com/microsoft/microsoft-ui-xaml/issues/1200
            CurrentBoard = newBoard;
            // Don't add to BoardList here.  Wait till user saves.
        }

        public void EditBoardCommandHandler()
        {
            TmpBoard = CurrentBoard;
            BoardEditorTitle = "Edit Board";
        }

        public void SaveBoardCommandHandler()
        {
            if (CurrentBoard.Board == null)
                return;

            BoardDTO dto = CurrentBoard.Board.To_BoardDTO();
            bool isNew = dto.Id == 0;
            RowOpResult<BoardDTO> result = null;

            // Add board to db and collection
            if (isNew)
                result = dataProvider.AddBoard(dto);
            else
                result = dataProvider.UpdateBoard(dto);

            messagePump.Show(result.Success ? "Board was saved successfully." : result.ErrorMessage, MessageDuration);

            if (isNew && result.Success)
            {
                CurrentBoard.Board.ID = result.Entity.Id;
                BoardList.Add(CurrentBoard);
            }
        }

        public void CancelSaveBoardCommandHandler()
        {
            CurrentBoard = null; 
            CurrentBoard = TmpBoard;
        }

        public void DeleteBoardCommandHandler()
        {
            if (CurrentBoard == null)
                return;

            dataProvider.DeleteBoard(CurrentBoard.Board.ID);
            BoardList.Remove(CurrentBoard);
            CurrentBoard = null; // uwp bug
            CurrentBoard = BoardList.LastOrDefault();
        }
    }
}
