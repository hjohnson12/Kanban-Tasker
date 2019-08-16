using KanbanTasker.Base;
using KanbanTasker.DataAccess;
using KanbanTasker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.ViewModels
{
    public class MainViewModel : Observable
    {
        private ObservableCollection<BoardViewModel> _boardList;
        private BoardViewModel _current;
        private string _boardName;
        private string _boardNotes;
        private ObservableCollection<CustomKanbanModel> allTasks;

        public BoardViewModel CreateDefaultBoard()
        {
            // Create default board
            BoardViewModel newBoard = new BoardViewModel
            {
                BoardName = "Default Board"
            };
            // Add to collection and db
            int newBoardId = DataProvider.AddBoard("New Board", "This is a default board added when there are none");
            newBoard.BoardId = newBoardId.ToString();
            newBoard.Tasks = new ObservableCollection<CustomKanbanModel>();
            return newBoard;
        }
        public MainViewModel()
        {
            // Instantiate the collection object
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
            //if (BoardList.Count == 0)
            //{
            //    // Create board
            //    BoardViewModel newBoard = new BoardViewModel
            //    {
            //        BoardName = "New Board"
            //    };

            //    // Add to collection and db
            //    int newBoardId = DataProvider.AddBoard("New Board", "This is a default board added when there are none");
            //    newBoard.BoardId = newBoardId.ToString();
            //    newBoard.Tasks = new ObservableCollection<CustomKanbanModel>();
            //    BoardList.Add(newBoard);
            //    Current = newBoard;

                
            //}
            //else
            //{
            //    allTasks = new ObservableCollection<CustomKanbanModel>();
            //    allTasks = DataProvider.GetData();

            //    // Sort according to ColumnIndex that way tasks are loaded 
            //    // in the correct places
            //    var sortedCollection = allTasks.OrderBy(x => x.ColumnIndex);

            //    foreach (var board in BoardList)
            //    {
            //        foreach (var task in sortedCollection)
            //        {
            //            if (task.BoardId == board.BoardId)
            //                board.Tasks.Add(task);
            //        }
            //    }
               
            // //   Current = BoardList[0];
            //}
        }

        public void CreateBoard()
        {
            // Create board
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
            //Current = newBoard;
            //return newBoard;
        }

        internal bool DeleteBoard(BoardViewModel currentBoard)
        {
            // Clear tasks collection and remove board from board list
            // Remove tasks from tblTasks and board from tblBoards in DataProvider
            currentBoard.Tasks.Clear();
            BoardList.Remove(currentBoard);
            return DataProvider.DeleteBoard(currentBoard.BoardId);
        }

        public ObservableCollection<BoardViewModel> BoardList
        {
            get
            {
                return _boardList;
            }
            set
            {
                _boardList = value;
                OnPropertyChanged("BoardList");
            }
        }

        public BoardViewModel Current 
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                OnPropertyChanged("Current"); // Let the UI know to update this binding.
            }

        }

        internal void UpdateBoard(BoardViewModel currentBoard)
        {
            var oldBoardName = currentBoard.BoardName;
            var newBoardName = BoardName;
            DataProvider.UpdateBoard(currentBoard.BoardId);
        }

        public string BoardName
        {
            get { return _boardName; }
            set
            {
                _boardName = value;
                OnPropertyChanged();
            }
        }


        public string BoardNotes
        {
            get { return _boardNotes; }
            set
            {
                _boardNotes = value;
                OnPropertyChanged();
            }
        }
    }
}
