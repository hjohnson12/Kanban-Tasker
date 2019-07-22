using KanbanTasker.Base;
using KanbanTasker.DataAccess;
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

        public MainViewModel()
        {
            // Instantiate the list object.  You bind the XAML list to this in 'ItemSource'
            BoardList = new ObservableCollection<BoardViewModel>();

            // Create board
            BoardViewModel myBoard = new BoardViewModel();
            myBoard.BoardName = "Test - Initial Board from Constructor";
            
            // Add to list
            BoardList.Add(myBoard);
            Current = myBoard;

            var anotherBoard = new BoardViewModel();
            BoardList.Add(anotherBoard);
            anotherBoard.BoardName = "This is another board for testing";
            anotherBoard.BoardNotes = "We created it in the MainViewModel Constructor.";
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
