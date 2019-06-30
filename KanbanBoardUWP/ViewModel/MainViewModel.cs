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

        private KanbanModel Model = new KanbanModel();

        public MainViewModel()
        {
            Tasks = DataAccess.GetData();
        }

        public string ID
        {
            get { return Model.ID; }
            set
            {
                Model.ID = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return Model.Title; }
            set
            {
                Model.Title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return Model.Description; }
            set
            {
                Model.Description = value;
                OnPropertyChanged();
            }
        }

        public object Category
        {
            get { return Model.Category; }
            set
            {
                Model.Category = value;
                OnPropertyChanged();
            }
        }

        public object ColorKey
        {
            get { return Model.ColorKey; }
            set
            {
                Model.ColorKey = value;
                OnPropertyChanged();
            }
        }


    }
}
