using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using Microsoft.EntityFrameworkCore;
using KanbanTasker.Model.Services;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.MSSQL
{
    public class TaskServices : BaseService, ITaskServices
    {
        public TaskServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }

        public virtual List<TaskDto> GetTasks() => db.Tasks.ToList();

        public virtual RowOpResult<TaskDto> SaveTask(TaskDto task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            RowOpResult<TaskDto> result = new RowOpResult<TaskDto>(task);

            ValidateTask(result);

            if (!result.Success)
                return result;

            db.Entry(task).State = task.Id == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();
            result.Success = true;
            return result;
        }

        public virtual RowOpResult DeleteTask(int id)
        {
            RowOpResult result = new RowOpResult();
            TaskDto task = db.Tasks.FirstOrDefault(x => x.Id == id);

            if (task == null)
            {
                result.ErrorMessage = $"taskId {id} is invalid.  Task may have already been deleted.";
                return result;
            }

            db.Entry(task).State = EntityState.Deleted;
            db.SaveChanges();
            result.Success = true;
            return result;
        }

        public virtual void UpdateColumnData(TaskDto task) => SaveTask(task);

        public virtual void UpdateCardIndex(int iD, int currentCardIndex)
        {
            TaskDto task = db.Tasks.First(x => x.Id == iD);
            task.ColumnIndex = currentCardIndex;
            SaveTask(task);
        }

        public void UpdateColumnName(int iD, string newColName)
        {
            throw new NotImplementedException();
        }
    }
}