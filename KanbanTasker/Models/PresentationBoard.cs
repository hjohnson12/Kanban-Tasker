using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanbanTasker.Model;

namespace KanbanTasker.Models
{
    public class PresentationBoard : Base.Observable
    {
        private int _Id;
        private string _Name;
        private string _Notes;
        

      //  public int ID { get => _Id; set { _Id = value; OnPropertyChanged(); } }
        public int ID
        {
            get => _Id;
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Notes
        {
            get { return _Notes; }
            set
            {
                if (_Notes != value)
                {
                    _Notes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PresentationTask> _Tasks;
        public ObservableCollection<PresentationTask> Tasks
        {
            get => _Tasks;
            set
            {
                if (_Tasks != value)
                {
                    _Tasks = value;
                    OnPropertyChanged();
                }
            }
        }

        public PresentationBoard(BoardDTO dto)
        {
            ID = dto.Id;
            Name = dto.Name;
            Notes = dto.Notes;
            Tasks = new ObservableCollection<PresentationTask>();

            if (dto.Tasks?.Any() ?? false)
                foreach (TaskDTO taskDto in dto.Tasks)
                    Tasks.Add(new PresentationTask(taskDto));
        }

        public BoardDTO To_BoardDTO()
        {
            return new BoardDTO
            {
                Id = ID,
                Name = Name,
                Notes = Notes,
                Tasks = Tasks.Select(x => x.To_TaskDTO()).ToList()
            };
        }
    }
}
