using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public interface IDatabaseServices
    {
        void BackupDB(string destPath);
    }
}
