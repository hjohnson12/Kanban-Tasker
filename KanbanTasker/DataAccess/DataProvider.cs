using KanbanTasker.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace KanbanTasker.DataAccess
{
    public static class DataProvider
    {
        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=kanbanMultiboard.db"))
            {
                db.Open();

                string tblTasksCommand = "CREATE TABLE IF NOT " +
                    "EXISTS tblTasks (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "BoardID INTEGER NULL, " +
                    "Title NVARCHAR(2048) NULL, " +
                    "Description NVARCHAR(2048) NULL, " +
                    "Category NVARCHAR(2048) NULL, " +
                    "ColorKey NVARCHAR(2048) NULL, " +
                    "Tags NVARCHAR(2048) NULL)";

                string tblBoardsCommand = "CREATE TABLE IF NOT " +
                    "EXISTS tblBoards (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name NVARCHAR(2048) NULL)";

                SqliteCommand createTblTasks = new SqliteCommand(tblTasksCommand, db);
                SqliteCommand createTblBoards = new SqliteCommand(tblBoardsCommand, db);
                createTblTasks.ExecuteReader();
                createTblBoards.ExecuteReader();
                db.Close();
            }
        }

        public static int AddTask(string boardID, string title, string desc, string categ, string colorKey, string tags)
        {
            long pd = -1;
            using (SqliteConnection db =
                new SqliteConnection("Filename=kanbanMultiboard.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,

                    // Use parameterized query to prevent SQL injection attacks
                    CommandText = "INSERT INTO tblTasks (BoardID,Title,Description,Category,ColorKey,Tags) VALUES (@boardID, @title, @desc, @categ, @colorKey, @tags); ; SELECT last_insert_rowid();"
                };
                insertCommand.Parameters.AddWithValue("@boardID", boardID);
                insertCommand.Parameters.AddWithValue("@title", title);
                insertCommand.Parameters.AddWithValue("@desc", desc);
                insertCommand.Parameters.AddWithValue("@categ", categ);
                insertCommand.Parameters.AddWithValue("@colorKey", colorKey);
                insertCommand.Parameters.AddWithValue("@tags", tags);
                pd = (long)insertCommand.ExecuteScalar();

                db.Close();
            }
            return (int)pd;
        }

        public static void DeleteTask(string id)
        {
            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection("Filename=kanbanMultiboard.db"))
            {
                db.Open();
                SqliteCommand deleteCommand = new SqliteCommand
                    ("DELETE FROM tblTasks WHERE Id=@id", db);
                deleteCommand.Parameters.AddWithValue("id", id);
                deleteCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        //=====================================================================
        // FUNCTIONS & EVENTS FOR EDITING A TASK
        //=====================================================================
        public static ObservableCollection<CustomKanbanModel> GetData()
        {
            ObservableCollection<CustomKanbanModel> tasks = new ObservableCollection<CustomKanbanModel>();

            // Get tasks and return the collection
            using (SqliteConnection db =
                new SqliteConnection("Filename=kanbanMultiboard.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id, BoardID, Title, Description, Category, ColorKey, Tags from tblTasks", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                // Query the db and get the tasks
                while (query.Read())
                {
                    string[] tags;
                    if (query.GetString(6).ToString() == "")
                        tags = new string[] { }; // Empty array if no tags are in the col
                    else
                        tags = query.GetString(6).Split(","); // Turn string of tags into string array, fills listview

                    CustomKanbanModel row = new CustomKanbanModel()
                    {
                        ID = query.GetString(0),
                        BoardId = query.GetString(1),
                        Title = query.GetString(2),
                        Description = query.GetString(3),
                        Category = query.GetString(4),
                        ColorKey = query.GetString(5),
                        Tags = tags // Turn string of tags into string array, fills listview
                    };

                    tasks.Add(row);
                }
                db.Close();
            }
            return tasks;
        }

        public static void UpdateTask(string id, string title, string descr, string category, string colorKey, string tags)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=kanbanMultiboard.db"))
            {
                db.Open();

                // Update item
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET Title=@title, Description=@desc, Category=@categ, ColorKey=@colorKey, Tags=@tags WHERE Id=@id", db);
                updateCommand.Parameters.AddWithValue("@title", title);
                updateCommand.Parameters.AddWithValue("@desc", descr);
                updateCommand.Parameters.AddWithValue("@categ", category);
                updateCommand.Parameters.AddWithValue("@colorKey", colorKey);
                updateCommand.Parameters.AddWithValue("@tags", tags);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }
    }
}