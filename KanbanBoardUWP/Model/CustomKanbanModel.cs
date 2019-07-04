using KanbanBoardUWP.Base;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanBoardUWP.Model
{
    public class CustomKanbanModel : Observable, IKanbanModel
    {
        private string _title;
        private Uri _imageUrl;
        private object _colorKey;
        private object _category;
        private string[] _tags;
        private string _description;
        private string _id;

        public string ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(); } }
        public string[] Tags { get { return _tags; } set { _tags = value; OnPropertyChanged(); } }
        public object Category { get { return _category; } set { _category = value; OnPropertyChanged(); } }
        public Uri ImageURL { get { return _imageUrl; } set { _imageUrl = value; OnPropertyChanged(); } }
        public object ColorKey { get { return _colorKey; } set { _colorKey = value; OnPropertyChanged(); } }
    }
}
