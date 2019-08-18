using KanbanTasker.Base;
using KanbanTasker.DataAccess;
using KanbanTasker.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace KanbanTasker.ViewModels
{
    public class MainViewModel : Observable
    {
        /// <summary>
        /// Backing fields
        /// </summary>
        private ObservableCollection<BoardViewModel> _boardList;
        private BoardViewModel _current;
        private string _boardName;
        private string _boardNotes;
        private ObservableCollection<CustomKanbanModel> allTasks;

        /// <summary>
        ///  Constructor / Initiliazation of boards and tasks.
        ///  Sorts the tasks by column index so that they are
        ///  loaded in as they were left when the app closed
        /// </summary>
        public MainViewModel()
        {
            BoardList = DataProvider.GetBoards();
            allTasks = new ObservableCollection<CustomKanbanModel>();
            allTasks = DataProvider.GetData();

            // Sort according to ColumnIndex that way tasks are loaded 
            // in the correct places
            var sortedCollection = allTasks.OrderBy(x => x.ColumnIndex);

            foreach (var board in BoardList)
            {
                foreach (var task in sortedCollection)
                {
                    if (task.BoardId == board.BoardId)
                        board.Tasks.Add(task);
                }
            }

        }

        /// <summary>
        /// Creates a new board with BoardName and BoardNotes. Adds
        /// it to the database and collection
        /// </summary>
        public void CreateBoard()
        {
            BoardViewModel newBoard = new BoardViewModel
            {
                BoardName = BoardName,
                BoardNotes = BoardNotes
            };

            // Add board to db and collection
            int newBoardId = DataProvider.AddBoard(BoardName, BoardNotes);
            newBoard.BoardId = newBoardId.ToString();
            newBoard.Tasks = new ObservableCollection<CustomKanbanModel>();
            BoardList.Add(newBoard);
        }

        /// <summary>
        /// Clears tasks collection and removes board from list.
        /// Then deletes the board and its tasks from the database
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <returns>If deletion was successful</returns>
        internal bool DeleteBoard(BoardViewModel currentBoard)
        {
            currentBoard.Tasks.Clear();
            BoardList.Remove(currentBoard);
            return DataProvider.DeleteBoard(currentBoard.BoardId);
        }

        /// <summary>
        /// Updates the current boards name and notes,
        /// then updates the list and database
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="currentIndex"></param>
        /// <returns>If updating was successful</returns>
        internal bool UpdateBoard(BoardViewModel currentBoard, int currentIndex)
        {
            currentBoard.BoardName = BoardName;
            currentBoard.BoardNotes = BoardNotes;
            BoardList[currentIndex] = currentBoard;
            return DataProvider.UpdateBoard(currentBoard.BoardId, BoardName, BoardNotes);
        }

        #region Properties

        /// <summary>
        /// List of all boards
        /// </summary>
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
        public BoardViewModel Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                OnPropertyChanged();
            }

        }

        /// <summary>
        /// Currently selected BoardName
        /// </summary>
        public string BoardName
        {
            get { return _boardName; }
            set
            {
                _boardName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Currently selected BoardNotes
        /// </summary>
        public string BoardNotes
        {
            get { return _boardNotes; }
            set
            {
                _boardNotes = value;
                OnPropertyChanged();
            }
        }
        #endregion Properties
    }
}
