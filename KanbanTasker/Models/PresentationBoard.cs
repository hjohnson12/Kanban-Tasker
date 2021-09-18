using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Models
{
    public class PresentationBoard : Base.Observable
    {
        private int _id;
        private string _name;
        private string _notes;
        private ObservableCollection<PresentationTask> _tasks;
        private ObservableCollection<string> _tagsCollection;

        public PresentationBoard(BoardDTO dto)
        {
            ID = dto.Id;
            Name = dto.Name;
            Notes = dto.Notes;
            Tasks = new ObservableCollection<PresentationTask>();
            TagsCollection = new ObservableCollection<string>();
        }

        /// <summary>
        /// The board ID
        /// </summary>
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Name of the kanban board
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// A description of the board
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        /// <summary>
        /// A collection of tasks pertaining to a board's instance
        /// </summary>
        public ObservableCollection<PresentationTask> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        /// <summary>
        /// A collection of tags for a task available to this board
        /// </summary>
        public ObservableCollection<string> TagsCollection 
        { 
            get => _tagsCollection;
            set => SetProperty(ref _tagsCollection, value);
        }

        public BoardDTO To_BoardDTO()
        {
            return new BoardDTO
            {
                Id = ID,
                Name = Name,
                Notes = Notes
                // do not convert tasks here since each task has a board, each board has one or more tasks, each of which have a board with one or more tasks.... ad infinitum.
                // the board also has its own set of tags
            };
        }
    }
}