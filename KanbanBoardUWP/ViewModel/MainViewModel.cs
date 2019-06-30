using KanbanBoardUWP.Base;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanBoardUWP.ViewModel
{
    public class MainViewModel : Observable
    {
        public ObservableCollection<KanbanModel> Tasks { get; }


        public MainViewModel()
        {
            Tasks = DataAccess.GetData();
        }
    }
}
