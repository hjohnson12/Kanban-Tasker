using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using KanbanTasker.Services.Database;
using Microsoft.Data.Sqlite;
using KanbanTasker.Model;

namespace KanbanTasker.Services.Database.Components.SQLite
{
    public class DatabaseInitializer : IDatabaseInitializer
    {

        private Db db;


        public DatabaseInitializer(Db db)
        {
            this.db = db;
            Seed(null).Wait();
        }

        public async Task Seed(string migrationName)
        {
            using (SqliteConnection db =  new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                string tblTasksCommand = "CREATE TABLE IF NOT " +
                    "EXISTS tblTasks (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "BoardID INTEGER NULL, " +
                    "DateCreated NVARCHAR(2048) NULL, " +
                    "Title NVARCHAR(2048) NULL, " +
                    "Description NVARCHAR(2048) NULL, " +
                    "Category NVARCHAR(2048) NULL, " +
                    "ColumnIndex INTEGER NULL, " +
                    "ColorKey NVARCHAR(2048) NULL, " +
                    "Tags NVARCHAR(2048) NULL)";

                string tblBoardsCommand = "CREATE TABLE IF NOT " +
                    "EXISTS tblBoards (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name NVARCHAR(2048) NULL, " +
                    "Notes NVARCHAR(2048) NULL)";

                string tblColumnsCommand = "CREATE TABLE IF NOT " +
                    "EXISTS tblColumns (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "BoardID INTEGER NULL, " +
                    "ColumnName NVARCHAR(2048) NULL, " +
                    "Indx INTEGER NULL)";

                SqliteCommand createTblTasks = new SqliteCommand(tblTasksCommand, db);
                SqliteCommand createTblBoards = new SqliteCommand(tblBoardsCommand, db);
                SqliteCommand createTblColumns = new SqliteCommand(tblColumnsCommand, db);

                createTblTasks.ExecuteReader();
                createTblBoards.ExecuteReader();
                createTblColumns.ExecuteReader(); 

                db.Close();
            }
        }
    }
}
