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

        public MainViewModel()
        {
            // Instantiate the collection object
            BoardList = DataProvider.GetBoards();

            if (BoardList.Count == 0)
            {
                // Create board
                BoardViewModel newBoard = new BoardViewModel
                {
                    BoardName = "New Board"
                };

                // Add to collection and db
                int newBoardId = DataProvider.AddBoard("New Board", "");
                newBoard.BoardId = newBoardId.ToString();
                newBoard.Tasks = new ObservableCollection<CustomKanbanModel>();
                BoardList.Add(newBoard);
                Current = newBoard;
            }
            else
            {
                allTasks = new ObservableCollection<CustomKanbanModel>();
                allTasks = DataProvider.GetData();
                foreach (var board in BoardList)
                {
                    foreach (var task in allTasks)
                    {
                        if (task.BoardId == board.BoardId)
                            board.Tasks.Add(task);
                    }
                }
               
                Current = BoardList[0];
            }
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
            foreach (var task in allTasks)
                if (task.BoardId == newBoardId.ToString())
                    newBoard.Tasks.Add(task);
            BoardList.Add(newBoard);
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
