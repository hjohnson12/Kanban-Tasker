using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public class Tag
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagBackground { get; set; }
        public string TagForeground { get; set; }
        public ICollection<TaskTag> TaskTags { get; set; }
    }
}
