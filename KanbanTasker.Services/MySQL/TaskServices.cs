using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;

namespace KanbanTasker.Services.MySQL
{
    public class TaskServices : MSSQL.TaskServices, ITaskServices
    {
        public TaskServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest) { }

        public override List<TaskDTO> GetTasks() => base.GetTasks();

        public override RowOpResult<TaskDTO> SaveTask(TaskDTO task) => base.SaveTask(task);

        public override RowOpResult DeleteTask(int id) => base.DeleteTask(id);

        public override void UpdateColumnData(TaskDTO task) => base.UpdateColumnData(task);

        public override void UpdateCardIndex(int iD, int currentCardIndex) => base.UpdateCardIndex(iD, currentCardIndex);
    }
}