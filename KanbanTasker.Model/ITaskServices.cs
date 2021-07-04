using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    /// <summary>
    /// Interface for a service that interacts with the tasks within a kanban board.
    /// </summary>
    public interface ITaskServices
    {
        RowOpResult<TaskDTO> SaveTask(TaskDTO task);
        RowOpResult DeleteTask(int id);
        List<TaskDTO> GetTasks();
        void UpdateCardIndex(int iD, int currentCardIndex);
        void UpdateColumnData(TaskDTO task);
        RowOpResult<TaskDTO> ValidateTask(RowOpResult<TaskDTO> result);
    }
}
