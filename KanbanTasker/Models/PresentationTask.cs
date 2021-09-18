using KanbanTasker.Model.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
        private string _timeDue;
        private string _startDate;
        private string _finishDate;
        private string _reminderTime;
        private string _daysWorkedOn;
        private string _daysSinceCreation;
        private int _id;
        private int _boardId;
        private PresentationBoard _board;

        public PresentationTask(TaskDto dto)
        {
            ID = dto.Id;
            BoardId = dto.BoardId;
            DateCreated = dto.DateCreated;
            DueDate = dto.DueDate;
            TimeDue = dto.TimeDue;
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

            Board = new PresentationBoard(dto?.Board ?? new BoardDto());
        }

        public PresentationBoard Board
        {
            get => _board;
            set => SetProperty(ref _board, value);
        }

        public int ID 
        { 
            get => _id;
            set => SetProperty(ref _id, value);
        }
        
        public int BoardId
        {
            get => _boardId;
            set => SetProperty(ref _boardId, value);
        }

        public string Title 
        { 
            get => _title;
            set => SetProperty(ref _title, value); 
        }

        public string Description 
        { 
            get => _description;
            set => SetProperty(ref _description, value); 
        }

        public string Category 
        { 
            get => _category;
            set => SetProperty(ref _category, value); 
        }

        public int ColumnIndex 
        { 
            get => _columnIndex;
            set => SetProperty(ref _columnIndex, value);
        }

        public string ColorKey 
        { 
            get => _colorKey;
            set => SetProperty(ref _colorKey, value);
        }

        public ObservableCollection<string> Tags 
        { 
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public Uri ImageURL 
        { 
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string DateCreated
        {
            get => _dateCreated;
            set => SetProperty(ref _dateCreated, value);
        }

        public string DueDate 
        { 
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        public string StartDate 
        { 
            get => _startDate;
            set => SetProperty(ref _startDate, value); 
        }

        public string FinishDate 
        { 
            get => _finishDate;
            set => SetProperty(ref _finishDate, value); 
        }

        public string ReminderTime 
        { 
            get => _reminderTime;
            set => SetProperty(ref _reminderTime, value); 
        }

        public string TimeDue 
        { 
            get => _timeDue;
            set => SetProperty(ref _timeDue, value); 
        }

        public string DaysWorkedOn 
        { 
            get => _daysWorkedOn;
            set => SetProperty(ref _daysWorkedOn, value); 
        }

        public string DaysSinceCreation 
        { 
            get => _daysSinceCreation;
            set => SetProperty(ref _daysSinceCreation, value); 
        }

        public TaskDto To_TaskDTO()
        {
            return new TaskDto
            {
                Id = ID,
                BoardId = BoardId,
                DateCreated = DateCreated,
                DueDate = DueDate,
                TimeDue = TimeDue,
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