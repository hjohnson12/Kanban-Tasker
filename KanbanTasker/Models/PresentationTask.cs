using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanbanTasker.Model;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Models
{
    public class PresentationTask : Base.Observable
    {
        private string _title;
        private Uri _imageUrl;
        private string _colorKey;
        private string _category;
        private int? _columnIndex;
        private ObservableCollection<string> _tags;
        private string _description;
        private string _dateCreated;
        private int _id;
        private int? _boardId;
        private PresentationBoard _Board;

        public int ID { get => _id; set { _id = value; OnPropertyChanged(); } }
        public int? BoardId { get => _boardId; set { _boardId = value; OnPropertyChanged(); } }
        public string DateCreated { get => _dateCreated; set { _dateCreated = value; OnPropertyChanged(); } }
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }
        public int? ColumnIndex { get => _columnIndex; set { _columnIndex = value; OnPropertyChanged(); } }
        public string ColorKey { get => _colorKey; set { _colorKey = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Tags { get => _tags; set { _tags = value; OnPropertyChanged(); } }
        public Uri ImageURL { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }
        public PresentationBoard Board { get => _Board; set { _Board = value; OnPropertyChanged(); } }

        public object ColorKeyComboBoxItem { get; set; }                                                    // Hack --- Combobox cannot bind correctly.

        public PresentationTask(TaskDTO dto)
        {
            ID = dto.Id;
            BoardId = dto.BoardId;
            DateCreated = dto.DateCreated;
            Title = dto.Title;
            Description = dto.Description;
            Category = dto.Category;
            ColumnIndex = dto.ColumnIndex;
            ColorKey = dto.ColorKey;

            if (!string.IsNullOrEmpty(dto.Tags))
                Tags = new ObservableCollection<string>(dto.Tags.Split(','));
            else
                Tags = new ObservableCollection<string>();

            Board = new PresentationBoard(dto?.Board ?? new BoardDTO());
        }

        public TaskDTO To_TaskDTO()
        {
            return new TaskDTO
            {
                Id = ID,
                BoardId = BoardId,
                DateCreated = DateCreated,
                Title = Title,
                Description = Description,
                Category = Category,
                ColumnIndex = ColumnIndex,
                ColorKey = ColorKey,
                Tags = Tags == null ? string.Empty : string.Join(",", Tags),
                Board = Board.To_BoardDTO()
            };
        }
    }
}
