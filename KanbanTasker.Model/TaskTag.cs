using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public class TaskTag
    {
        public int TaskID { get; set; }
        public int TagID { get; set; }
        public virtual TaskDTO Task { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
