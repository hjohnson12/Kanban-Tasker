using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;

namespace KanbanTasker.DataAccess
{
    public static class DataProvider
    {
        private const string DBName = "Filename=ktdatabase.db";

        /// <summary>
        /// Initialize database tables on application startup
        /// </summary>
        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
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

        /// <summary>
        /// Queries the database for each task in tblTasks and 
        /// adds them into a collection to fill a boards tasks
        /// </summary>
        /// <returns>Collection of tasks, of type CustomKanbanModel</returns>
        public static ObservableCollection<CustomKanbanModel> GetData()
        {
            ObservableCollection<CustomKanbanModel> tasks = new ObservableCollection<CustomKanbanModel>();

            using (SqliteConnection db =
                new SqliteConnection(DBName))
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

        /// <summary>
        /// Queries the database for each board in tblBoards
        /// and returns a collection of boards
        /// </summary>
        /// <returns>Collection of boards, of type BoardViewModel</returns>
        public static ObservableCollection<BoardViewModel> GetBoards()
        {
            ObservableCollection<BoardViewModel> boards = new ObservableCollection<BoardViewModel>();

            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id,Name,Notes from tblBoards", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

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

        /// <summary>
        /// Adds board to the database and generates the next
        /// ID in the sequence
        /// Uses parameterized query to prevent SQL injection attacks
        /// </summary>
        /// <param name="boardName"></param>
        /// <param name="boardNotes"></param>
        /// <returns>int, the newest board id in the sequence</returns>
        public static int AddBoard(string boardName, string boardNotes)
        {
            long pd = -1;
            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "INSERT INTO tblBoards (Name,Notes) VALUES (@boardName, @boardNotes); ; SELECT last_insert_rowid();"
                };
                insertCommand.Parameters.AddWithValue("@boardName", boardName);
                insertCommand.Parameters.AddWithValue("@boardNotes", boardNotes);
                pd = (long)insertCommand.ExecuteScalar();

                db.Close();
            }
            return (int)pd;
        }

        /// <summary>
        /// Adds task to database and generates the next
        /// ID in the sequence
        /// </summary>
        /// <param name="boardID"></param>
        /// <param name="dateCreated"></param>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        /// <param name="categ"></param>
        /// <param name="colorKey"></param>
        /// <param name="tags"></param>
        /// <returns>int, the newest task id in the sequence </returns>
        public static (int, bool) AddTask(string boardID, string dateCreated, string title, string desc, string categ, string colorKey, string tags)
        {
            long pd = -1;
            var success = true;
            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();
                try
                {
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
                    success = true;
                }
                catch (Exception ex) { success = false; }
                finally { db.Close(); }
            }
            return ((int)pd, success);
        }

        /// <summary>
        /// Deletes the tasks associated with the current board first
        /// and then removes the board from tblBoards
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns>If deletion was successful</returns>
        internal static bool DeleteBoard(string boardId)
        {
            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection(DBName))
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
                catch (Exception ex)
                {
                    return false;
                }
                finally { db.Close(); }
            }
        }

        /// <summary>
        /// Deletes task from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>If deletion was successful</returns>
        public static bool DeleteTask(string id)
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                try
                {
                    SqliteCommand deleteCommand = new SqliteCommand
                    ("DELETE FROM tblTasks WHERE Id=@id", db);
                    deleteCommand.Parameters.AddWithValue("id", id);
                    deleteCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { return false; }
                finally { db.Close(); }
            }
        }

        /// <summary>
        /// Updates a specific task in the database when user saves
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="descr"></param>
        /// <param name="category"></param>
        /// <param name="colorKey"></param>
        /// <param name="tags"></param>
        /// <returns>If update was successful</returns>
        public static bool UpdateTask(string id, string title, string descr, string category, string colorKey, string tags)
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                try
                {
                    SqliteCommand updateCommand = new SqliteCommand
                        ("UPDATE tblTasks SET Title=@title, Description=@desc, Category=@categ, ColorKey=@colorKey, Tags=@tags WHERE Id=@id", db);
                    updateCommand.Parameters.AddWithValue("@title", title);
                    updateCommand.Parameters.AddWithValue("@desc", descr);
                    updateCommand.Parameters.AddWithValue("@categ", category);
                    updateCommand.Parameters.AddWithValue("@colorKey", colorKey);
                    updateCommand.Parameters.AddWithValue("@tags", tags);
                    updateCommand.Parameters.AddWithValue("@id", id);
                    updateCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { return false; }
                finally { db.Close(); }
            }
        }

        /// <summary>
        /// Updates the selected card category and column index in the database after dragging
        /// it to a new column    
        /// </summary>
        /// <param name="selectedCardModel"></param>
        /// <param name="targetCategory"></param>
        /// <param name="targetIndex"></param>
        public static void UpdateColumnData(CustomKanbanModel selectedCardModel, string targetCategory, string targetIndex)
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
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

        /// <summary>
        /// Updates a specific card index in the database while reordering after dragging a card
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="currentCardIndex"></param>
        internal static void UpdateCardIndex(string iD, int currentCardIndex)
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
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

        /// <summary>
        /// Updates the current boards name and notes in the database
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="boardName"></param>
        /// <param name="boardNotes"></param>
        /// <returns></returns>
        internal static bool UpdateBoard(string boardId, string boardName, string boardNotes)
        {
            using (SqliteConnection db =
               new SqliteConnection(DBName))
            {
                db.Open();

                try
                {
                    // Update card  index
                    SqliteCommand updateCommand = new SqliteCommand
                        ("UPDATE tblBoards SET Name=@boardName,Notes=@boardNotes WHERE Id=@boardId", db);

                    updateCommand.Parameters.AddWithValue("@boardId", boardId);
                    updateCommand.Parameters.AddWithValue("@boardName", boardName);
                    updateCommand.Parameters.AddWithValue("@boardNotes", boardNotes);
                    updateCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { return false; }
                finally { db.Close(); }
            }
        }
    }
}