using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    public interface IDatabaseServices
    {
        void BackupDB(string destPath);
    }
}