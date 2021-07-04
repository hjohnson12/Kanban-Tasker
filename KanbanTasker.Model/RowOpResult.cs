using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public class RowOpResult<T> : RowOpResult
    {
        public T Entity { get; set; }

        public RowOpResult(T entity)
        {
            Entity = entity;
        }
    }

    public class RowOpResult
    {
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
    }
}
