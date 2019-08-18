using KanbanTasker.Base;
using Syncfusion.UI.Xaml.Kanban;
using System;

namespace KanbanTasker.Models
{
    public class CustomKanbanModel : Observable, IKanbanModel
    {
        private string _title;
        private Uri _imageUrl;
        private object _colorKey;
        private object _category;
        private string _columnIndex;
        private string[] _tags;
        private string _description;
        private string _dateCreated;
        private string _id;
        private string _boardId;

        //=====================================================================
        // Common KanbanModel Property Interface Members Required
        //=====================================================================

        public string ID { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string BoardId { get => _boardId; set { _boardId = value; OnPropertyChanged(); } }
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public string[] Tags { get => _tags; set { _tags = value; OnPropertyChanged(); } }
        public object Category { get => _category; set { _category = value; OnPropertyChanged(); } }
        public string ColumnIndex { get => _columnIndex; set { _columnIndex = value; OnPropertyChanged(); } }
        public Uri ImageURL { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }
        public object ColorKey { get => _colorKey; set { _colorKey = value; OnPropertyChanged(); } }

        public string DateCreated { get => _dateCreated; set { _dateCreated = value; OnPropertyChanged(); } }
    }
}
