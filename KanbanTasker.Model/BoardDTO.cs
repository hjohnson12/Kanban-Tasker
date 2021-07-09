using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    /// <summary>
    /// A data transfer object for a kanban board
    /// </summary>
    public class BoardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public virtual ICollection<TaskDTO> Tasks { get; set; }
    }
}
