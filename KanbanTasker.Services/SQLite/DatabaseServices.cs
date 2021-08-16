using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace KanbanTasker.Services.SQLite
{
    public class DatabaseServices: BaseService, IDatabaseServices
    {
        public DatabaseServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }

        /// <summary>
        /// Queries the database for each board in tblBoards
        /// and returns a collection of boards
        /// </summary>
        /// <returns>Collection of boards, of type BoardViewModel</returns>
        public void BackupDB(string destPath)
        {
            using (SqliteConnection db =
               new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                // Backup Db. Note, we need a path from where the user chooses to save the db
                //db.BackupDatabase()

                db.Close();
            }
        }
    }
}

