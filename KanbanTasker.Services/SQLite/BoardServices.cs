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
    public class BoardServices: BaseService, IBoardServices
    {
        public BoardServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }
         
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

        /// <summary>
        /// Queries the database for each board in tblBoards
        /// and returns a collection of boards
        /// </summary>
        /// <returns>Collection of boards, of type BoardViewModel</returns>
        public List<BoardDTO> GetBoards()
        {
            List<BoardDTO> boards = new List<BoardDTO>();
            List<TaskDTO> tasks = ServiceManifest.TaskServices.GetTasks();

            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
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
       /// Inserts the passed Board object if the ID is zero, otherwise updates it.
       /// </summary>
       /// <param name="board"></param>
       /// <returns></returns>
        public RowOpResult<BoardDTO> SaveBoard(BoardDTO board)
        {
            RowOpResult<BoardDTO> result = new RowOpResult<BoardDTO>(board);

            ValidateBoard(result);

            if (!result.Success)
                return result;
            
            using (SqliteConnection db = new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    db.Open();
                    SqliteCommand command = new SqliteCommand { Connection = db };
                    command.Parameters.AddWithValue("@boardName", board.Name);
                    command.Parameters.AddWithValue("@boardNotes", board.Notes);

                    if (board.Id == 0)
                    {
                        command.CommandText = "INSERT INTO tblBoards (Name,Notes) VALUES (@boardName, @boardNotes); ; SELECT last_insert_rowid();";
                        board.Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@boardId", board.Id);
                        command.CommandText = "UPDATE tblBoards SET Name=@boardName,Notes=@boardNotes WHERE Id=@boardId";
                        command.ExecuteNonQuery();

                    }
                    result.Success = true;
                }
                finally
                {
                    db.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes the tasks associated with the current board first
        /// and then removes the board from tblBoards
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns>If deletion was successful</returns>
        public RowOpResult DeleteBoard(int boardId)
        {
            RowOpResult result = new RowOpResult();

            // Delete task from db
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
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

                    result.Success = true;
                    return result;
                }
                
                finally
                {
                    db.Close();
                }
            }
        }
    }
}

