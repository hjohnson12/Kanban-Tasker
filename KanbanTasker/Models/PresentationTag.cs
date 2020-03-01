using KanbanTasker.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Models
{
    public class PresentationTag : Base.Observable
    {
        private int _Id;
        private string _tagName;
        private string _tagBackground;
        private string _tagForeground;
        private ICollection<TaskTag> _taskTags;

        public ICollection<TaskTag> TaskTags
        {
            get => _taskTags;
            set
            {
                if(_taskTags != value)
                {
                    _taskTags = value;
                    OnPropertyChanged();
                }
            }
        }

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
        public string TagName
        {
            get => _tagName;
            set
            {
                if (_tagName != value)
                {
                    _tagName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TagBackground
        {
            get => _tagBackground;
            set
            {
                if (_tagBackground != value)
                {
                    _tagBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TagForeground
        {
            get => _tagForeground;
            set
            {
                if (_tagForeground != value)
                {
                    _tagForeground = value;
                    OnPropertyChanged();
                }
            }
        }

        public PresentationTag(Tag dto)
        {
            ID = dto.Id;
            TagName = dto.TagName;
            TagBackground = dto.TagBackground;
            TagForeground = dto.TagForeground;
            TaskTags = dto.TaskTags;
        }

        public Tag To_TagDTO()
        {
            return new Tag
            {
                Id = ID,
                TagName = TagName,
                TagBackground = TagBackground,
                TagForeground = TagForeground
            };
        }
    }
}
