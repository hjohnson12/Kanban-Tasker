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
        private int _columnIndex;
        private ObservableCollection<string> _tags;
        private string _description;
        private string _dateCreated;
        private string _dueDate;
        private string _startDate;
        private string _finishDate;
        private string _reminderTime;
        private string _daysWorkedOn;
        private string _daysSinceCreation;
        private string _selectedTag;
        private int _id;
        private int _boardId;
        private PresentationBoard _Board;

        public int ID { get => _id; set { if (_id != value) { _id = value; OnPropertyChanged(); } } }
  //      public int? BoardId { get => _boardId; set { _boardId = value; OnPropertyChanged(); } }
        public int BoardId
        {
            get => _boardId;
            set
            {
                if(_boardId != value)
                {
                    _boardId = value;
                    OnPropertyChanged();
                }
            }
        }
     //   public string DateCreated { get => _dateCreated; set { _dateCreated = value; OnPropertyChanged(); } }
        public string DateCreated
        {
            get => _dateCreated;
            set
            {
                if (_dateCreated != value)
                {
                    _dateCreated = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DueDate { get => _dueDate; set { if (_dueDate != value) { _dueDate = value; OnPropertyChanged(); } } }
        public string StartDate { get => _startDate; set { if (_startDate != value) { _startDate = value; OnPropertyChanged(); } } }
        public string FinishDate { get => _finishDate; set { if (_finishDate != value) { _finishDate = value; OnPropertyChanged(); } } }
        public string ReminderTime { get => _reminderTime; set { if (_reminderTime != value) { _reminderTime = value; OnPropertyChanged(); } } }
        public string DaysWorkedOn { get => _daysWorkedOn; set { if (_daysWorkedOn != value) { _daysWorkedOn = value; OnPropertyChanged(); } } }
        public string DaysSinceCreation { get => _daysSinceCreation; set { if (_daysSinceCreation != value) { _daysSinceCreation = value; OnPropertyChanged(); } } }
        public string SelectedTag { get => _selectedTag; set { if (_selectedTag != value) { _selectedTag = value; OnPropertyChanged(); } } }

        public string Title { get => _title; set { if (_title != value) { _title = value; OnPropertyChanged(); } } }
        public string Description { get => _description; set { if(_description != value) { _description = value; OnPropertyChanged(); } } }
        public string Category { get => _category; set { if (_category != value) { _category = value; OnPropertyChanged(); } } }
        public int ColumnIndex { get => _columnIndex; set { if (_columnIndex != value) { _columnIndex = value; OnPropertyChanged(); } } }
        public string ColorKey { get => _colorKey; set { if (_colorKey != value) { _colorKey = value; OnPropertyChanged(); } } }
        public ObservableCollection<string> Tags { get => _tags; set { if (_tags != value) { _tags = value; OnPropertyChanged(); } } }
        public Uri ImageURL { get => _imageUrl; set { if (_imageUrl != value){ _imageUrl = value; OnPropertyChanged(); } } }
        public PresentationBoard Board { get => _Board; set { if(_Board != value){ _Board = value; OnPropertyChanged(); } } }

        private object _ColorKeyComboBoxItem;
        public object ColorKeyComboBoxItem
        {
            get => _ColorKeyComboBoxItem;
            set
            {
                if (_ColorKeyComboBoxItem != value)
                {
                    ColorKey = value == null ? null : ((ComboBoxItem)value).Content?.ToString();
                    _ColorKeyComboBoxItem = value;
                    OnPropertyChanged();
                }
            }
        }                                                    // Hack --- Combobox cannot bind correctly.

        public PresentationTask(TaskDTO dto)
        {
            ID = dto.Id;
            BoardId = dto.BoardId;
            DateCreated = dto.DateCreated;
            DueDate = dto.DueDate;
            ReminderTime = dto.ReminderTime;
            StartDate = dto.StartDate;
            FinishDate = dto.FinishDate;
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
                DueDate = DueDate,
                ReminderTime = ReminderTime,
                StartDate = StartDate,
                FinishDate = FinishDate,
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
