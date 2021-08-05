using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public class ColumnDTO
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public string ColumnName { get; set; }
        public int Indx { get; set; }
    }
}