using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;

namespace KanbanTasker.DataAccess
{
    public static class DataProvider
    {
        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
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

                SqliteCommand createTblTasks = new SqliteCommand(tblTasksCommand, db);
                SqliteCommand createTblBoards = new SqliteCommand(tblBoardsCommand, db);
                createTblTasks.ExecuteReader();
                createTblBoards.ExecuteReader();
                db.Close();
            }
        }

        public static void UpdateColumnData(CustomKanbanModel selectedCardModel, string targetCategory, string targetIndex)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                // Update task column/category when dragged to new column/category
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET Category=@category, ColumnIndex=@columnIndex WHERE Id=@id", db);
                updateCommand.Parameters.AddWithValue("@category", targetCategory);
                updateCommand.Parameters.AddWithValue("@columnIndex", targetIndex);
                updateCommand.Parameters.AddWithValue("@id", selectedCardModel.ID);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        public static int AddTask(string boardID, string dateCreated, string title, string desc, string categ, string colorKey, string tags)
        {
            long pd = -1;
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,

                    // Use parameterized query to prevent SQL injection attacks
                    CommandText = "INSERT INTO tblTasks (BoardID,DateCreated,Title,Description,Category,ColorKey,Tags) VALUES (@boardID, @dateCreated, @title, @desc, @categ, @colorKey, @tags); ; SELECT last_insert_rowid();"
                };
                insertCommand.Parameters.AddWithValue("@boardID", boardID);
                insertCommand.Parameters.AddWithValue("@dateCreated", dateCreated);
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

        internal static bool DeleteBoardTasks(string boardId)
        {
            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();
                try
                {
                    SqliteCommand deleteCommand = new SqliteCommand
                   ("DELETE FROM tblTasks WHERE BoardID=@boardId", db);
                    deleteCommand.Parameters.AddWithValue("boardId", boardId);
                    deleteCommand.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally { db.Close(); }
            }
        }

        internal static bool DeleteBoard(string boardId)
        {
            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();
                try
                {
                    SqliteCommand deleteCommand = new SqliteCommand
                   ("DELETE FROM tblTasks WHERE BoardID=@boardId", db);
                    deleteCommand.Parameters.AddWithValue("boardId", boardId);
                    deleteCommand.ExecuteNonQuery();

                    deleteCommand = new SqliteCommand
                   ("DELETE FROM tblBoards WHERE Id=@id", db);
                    deleteCommand.Parameters.AddWithValue("id", boardId);
                    deleteCommand.ExecuteNonQuery();

                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
                finally { db.Close(); }
            }
        }

        internal static void UpdateBoard(string boardId)
        {
            throw new NotImplementedException();
        }

        public static int AddBoard(string boardName, string boardNotes)
        {
            long pd = -1;
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,

                    // Use parameterized query to prevent SQL injection attacks
                    CommandText = "INSERT INTO tblBoards (Name,Notes) VALUES (@boardName, @boardNotes); ; SELECT last_insert_rowid();"
                };
                insertCommand.Parameters.AddWithValue("@boardName", boardName);
                insertCommand.Parameters.AddWithValue("@boardNotes", boardNotes);
                pd = (long)insertCommand.ExecuteScalar();

                db.Close();
            }
            return (int)pd;
        }

        public static void DeleteTask(string id)
        {
            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();
                SqliteCommand deleteCommand = new SqliteCommand
                    ("DELETE FROM tblTest WHERE Id=@id", db);
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
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id, BoardID, DateCreated, Title, Description, Category, ColumnIndex, ColorKey, Tags from tblTasks", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                // Query the db and get the tasks
                while (query.Read())
                {
                    string[] tags;
                    if (query.GetString(8).ToString() == "")
                        tags = new string[] { }; // Empty array if no tags are in the col
                    else
                        tags = query.GetString(8).Split(","); // Turn string of tags into string array, fills listview

                    CustomKanbanModel row = new CustomKanbanModel()
                    {
                        ID = query.GetString(0),
                        BoardId = query.GetString(1),
                        DateCreated = query.GetString(2),
                        Title = query.GetString(3),
                        Description = query.GetString(4),
                        Category = query.GetString(5),
                        ColumnIndex = query.GetString(6),
                        ColorKey = query.GetString(7),
                        Tags = tags // Turn string of tags into string array, fills listview
                    };

                    tasks.Add(row);
                }
                db.Close();
            }
            return tasks;
        }

        public static ObservableCollection<BoardViewModel> GetBoards()
        {
            ObservableCollection<BoardViewModel> boards = new ObservableCollection<BoardViewModel>();

            // Get tasks and return the collection
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id,Name,Notes from tblBoards", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                // Query the db and get the tasks
                while (query.Read())
                {
                    BoardViewModel row = new BoardViewModel()
                    {
                        BoardId = query.GetString(0),
                        BoardName = query.GetString(1),
                        BoardNotes = query.GetString(2),
                    };
                    boards.Add(row);
                }
                db.Close();
            }
            return boards;
        }

        public static void UpdateTask(string id, string title, string descr, string category, string colorKey, string tags)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
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

        internal static void UpdateCardIndex(string iD, int currentCardIndex)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=ktdatabase2.db"))
            {
                db.Open();

                // Update card  index
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET ColumnIndex=@columnIndex WHERE Id=@id", db);
          
                updateCommand.Parameters.AddWithValue("@id", iD);
                updateCommand.Parameters.AddWithValue("@columnIndex", currentCardIndex);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }
    }
}