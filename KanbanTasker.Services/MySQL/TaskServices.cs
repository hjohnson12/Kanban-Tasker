using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using KanbanTasker.Model.Services;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.MySQL
{
    public class TaskServices : MSSQL.TaskServices, ITaskServices
    {
        public TaskServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest) { }

        public override List<TaskDto> GetTasks() => base.GetTasks();

        public override RowOpResult<TaskDto> SaveTask(TaskDto task) => base.SaveTask(task);

        public override RowOpResult DeleteTask(int id) => base.DeleteTask(id);

        public override void UpdateColumnData(TaskDto task) => base.UpdateColumnData(task);

        public override void UpdateCardIndex(int iD, int currentCardIndex) => base.UpdateCardIndex(iD, currentCardIndex);
    }
}