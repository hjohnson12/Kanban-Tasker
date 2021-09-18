using KanbanTasker.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    /// <summary>
    /// Interface for a service that interacts with the tasks within a kanban board.
    /// </summary>
    public interface ITaskServices
    {
        RowOpResult<TaskDto> SaveTask(TaskDto task);
        RowOpResult DeleteTask(int id);
        List<TaskDto> GetTasks();
        void UpdateCardIndex(int iD, int currentCardIndex);
        void UpdateColumnData(TaskDto task);
        void UpdateColumnName(int iD, string newColName);
        RowOpResult<TaskDto> ValidateTask(RowOpResult<TaskDto> result);
    }
}