using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;

namespace KanbanTasker.Services
{
    public class DataProvider_SQLite : IKanbanTaskerService
    {
        private const string DBName = "Filename=ktdatabase.db";

        public DataProvider_SQLite()
        {

        }

        /// <summary>
        /// Static so it executes only once
        /// Initialize database tables on application startup
        /// </summary>
        static DataProvider_SQLite()
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
        public List<TaskDTO> GetTasks()
        {
            List<TaskDTO> tasks = new List<TaskDTO>();

            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id, BoardID, DateCreated, Title, Description, Category, ColumnIndex, ColorKey, Tags from tblTasks order by ColumnIndex", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                // Query the db and get the tasks
                while (query.Read())
                {
                    //string[] tags;
                    //if (query.GetString(8).ToString() == "")
                    //    tags = new string[] { }; // Empty array if no tags are in the col
                    //else
                    //    tags = query.GetString(8).Split(','); // Turn string of tags into string array, fills listview

                    TaskDTO row = new TaskDTO()
                    {
                        Id = Convert.ToInt32(query.GetString(0)),
                        BoardId = new Nullable<int>(Convert.ToInt32(query.GetString(1))),
                        DateCreated = query.GetString(2),
                        Title = query.GetString(3),
                        Description = query.GetString(4),
                        Category = query.GetString(5),
                        ColumnIndex = new Nullable<int>(Convert.ToInt32(query.GetValue(6) == DBNull.Value ? "0" : query.GetString(6))),
                        ColorKey = query.GetString(7),
                        Tags = query.GetString(8) 
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
        public List<BoardDTO> GetBoards()
        {
            List<BoardDTO> boards = new List<BoardDTO>();
            List<TaskDTO> tasks = GetTasks();

            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Id,Name,Notes from tblBoards", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    BoardDTO row = new BoardDTO()
                    {
                        Id = Convert.ToInt32(query.GetString(0)),
                        Name = query.GetString(1),
                        Notes = query.GetString(2),
                    };
                    boards.Add(row);
                }
                db.Close();
            }

            // populate tasks for each board
            foreach (BoardDTO board in boards)
                board.Tasks = tasks.Where(x => x.BoardId == board.Id).ToList();

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
        public RowOpResult<BoardDTO> AddBoard(BoardDTO board)
        {
            RowOpResult<BoardDTO> result = new RowOpResult<BoardDTO>(board);
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
                insertCommand.Parameters.AddWithValue("@boardName", board.Name);
                insertCommand.Parameters.AddWithValue("@boardNotes", board.Notes);
                pd = (long)insertCommand.ExecuteScalar();
                board.Id = (int)pd;

                db.Close();
            }
            result.Success = true;
            return result;
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
        public RowOpResult<TaskDTO> AddTask(TaskDTO task)
        {
            RowOpResult<TaskDTO> result = new RowOpResult<TaskDTO>(task);
            long pd = -1;

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
                    insertCommand.Parameters.AddWithValue("@boardID", task.BoardId);
                    insertCommand.Parameters.AddWithValue("@dateCreated", task.DateCreated);
                    insertCommand.Parameters.AddWithValue("@title", task.Title);
                    insertCommand.Parameters.AddWithValue("@desc", task.Description);
                    insertCommand.Parameters.AddWithValue("@categ", task.Category);
                    insertCommand.Parameters.AddWithValue("@colorKey", task.ColorKey);
                    insertCommand.Parameters.AddWithValue("@tags", task.Tags);
                    pd = (long)insertCommand.ExecuteScalar();
                    task.Id = (int)pd;
                    result.Success = true;
                }
                catch (Exception ex) { result.Success = false; }
                finally { db.Close(); }
            }
            return result;
        }

        /// <summary>
        /// Deletes the tasks associated with the current board first
        /// and then removes the board from tblBoards
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns>If deletion was successful</returns>
        public bool DeleteBoard(int boardId)
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
        public bool DeleteTask(int id)
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
        public RowOpResult<TaskDTO> UpdateTask(TaskDTO task)
        {
            RowOpResult<TaskDTO> result = new RowOpResult<TaskDTO>(task);

            ValidateTask(result);

            if (!result.Success)
                return result;


            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                try
                {
                    SqliteCommand updateCommand = new SqliteCommand
                        ("UPDATE tblTasks SET Title=@title, Description=@desc, Category=@categ, ColorKey=@colorKey, Tags=@tags WHERE Id=@id", db);
                    updateCommand.Parameters.AddWithValue("@title", task.Title);
                    updateCommand.Parameters.AddWithValue("@desc", task.Description);
                    updateCommand.Parameters.AddWithValue("@categ", task.Category);
                    updateCommand.Parameters.AddWithValue("@colorKey", task.ColorKey);
                    updateCommand.Parameters.AddWithValue("@tags", task.Tags);
                    updateCommand.Parameters.AddWithValue("@id", task.Id);
                    updateCommand.ExecuteNonQuery();
                    result.Success = true;
                }
                catch (Exception ex) { result.Success = false; }
                finally { db.Close(); }
            }
            return result;
        }

        /// <summary>
        /// Updates the selected card category and column index in the database after dragging
        /// it to a new column    
        /// </summary>
        /// <param name="selectedCardModel"></param>
        /// <param name="targetCategory"></param>
        /// <param name="targetIndex"></param>
        public void UpdateColumnData(TaskDTO task)
        {
            using (SqliteConnection db =
                new SqliteConnection(DBName))
            {
                db.Open();

                // Update task column/category when dragged to new column/category
                SqliteCommand updateCommand = new SqliteCommand
                    ("UPDATE tblTasks SET Category=@category, ColumnIndex=@columnIndex WHERE Id=@id", db);
                updateCommand.Parameters.AddWithValue("@category", task.Category);
                updateCommand.Parameters.AddWithValue("@columnIndex", task.ColumnIndex);
                updateCommand.Parameters.AddWithValue("@id", task.Id);
                updateCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        /// <summary>
        /// Updates a specific card index in the database while reordering after dragging a card
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="currentCardIndex"></param>
        public void UpdateCardIndex(int iD, int currentCardIndex)
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
        public RowOpResult<BoardDTO> UpdateBoard(BoardDTO board)
        {
            RowOpResult<BoardDTO> result = new RowOpResult<BoardDTO>(board);

            ValidateBoard(result);

            if (!result.Success)
                return result;



            using (SqliteConnection db =
               new SqliteConnection(DBName))
            {
                db.Open();

                try
                {
                    // Update card  index
                    SqliteCommand updateCommand = new SqliteCommand
                        ("UPDATE tblBoards SET Name=@boardName,Notes=@boardNotes WHERE Id=@boardId", db);

                    updateCommand.Parameters.AddWithValue("@boardId", board.Id);
                    updateCommand.Parameters.AddWithValue("@boardName", board.Name);
                    updateCommand.Parameters.AddWithValue("@boardNotes", board.Notes);
                    updateCommand.ExecuteNonQuery();
                    result.Success = true;
                }
                catch (Exception ex) { result.Success = false; }
                finally { db.Close(); }
            }
            return result;
        }

        public RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result)
        {
            BoardDTO b = result.Entity;

            if (string.IsNullOrEmpty(b.Name))
                result.ErrorMessage = "Name is required.";
            else if(b.Name.Length > 100)
                result.ErrorMessage = "Name is too long.";

            // more validation here
            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }

        public RowOpResult<TaskDTO> ValidateTask(RowOpResult<TaskDTO> result)
        {
            TaskDTO t = result.Entity;

            if (string.IsNullOrEmpty(t.Title))
                result.ErrorMessage = "Title is required.";
            else if (t.Title.Length > 100)
                result.ErrorMessage = "Title is too long.";

            // more validation here
            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }
    }
}