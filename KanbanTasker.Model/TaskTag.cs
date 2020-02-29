using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public class TaskTag
    {
        public int TaskID { get; set; }
        public int TagID { get; set; }
        public TaskDTO Task { get; set; }
        public Tag Tag { get; set; }
    }
}
