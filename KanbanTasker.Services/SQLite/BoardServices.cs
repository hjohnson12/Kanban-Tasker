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

        public List<ColumnDTO> GetColumnNames(int boardId)
        {
            List<ColumnDTO> columnNames = new List<ColumnDTO>();

            // Get column names from db
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    SqliteCommand selectCommand1 = new SqliteCommand
                        ("SELECT count(*) from tblColumns", db);

                    SqliteDataReader query = selectCommand1.ExecuteReader();

                    int count = 0;
                    while (query.Read())
                    {
                        count = Convert.ToInt32(query.GetString(0));
                    }

                    ColumnDTO columnOne, columnTwo, columnThree, columnFour, columnFive;

                    if (count == 0)
                    {
                        var boards = ServiceManifest.BoardServices.GetBoards();

                        // Set default columns

                        if (boards.Count != 0)
                        {
                            columnOne = new ColumnDTO
                            {
                                BoardId = boards[0].Id,
                                ColumnName = "Backlog",
                                Indx = 0
                            };

                            columnTwo = new ColumnDTO
                            {
                                BoardId = boards[0].Id,
                                ColumnName = "To Do",
                                Indx = 1
                            };

                            columnThree = new ColumnDTO
                            {
                                BoardId = boards[0].Id,
                                ColumnName = "In Progress",
                                Indx = 2
                            };

                            columnFour = new ColumnDTO
                            {
                                BoardId = boards[0].Id,
                                ColumnName = "Review",
                                Indx = 3
                            };

                            columnFive = new ColumnDTO
                            {
                                BoardId = boards[0].Id,
                                ColumnName = "Completed",
                                Indx = 4
                            };

                            // Insert columns and return id
                            SqliteCommand insertCommand = new SqliteCommand { Connection = db };
                            insertCommand.Parameters.AddWithValue("@boardId", columnOne.BoardId);
                            insertCommand.Parameters.AddWithValue("@columnName", columnOne.ColumnName);
                            insertCommand.Parameters.AddWithValue("@indx", columnOne.Indx);
                            insertCommand.CommandText = 
                                "INSERT INTO tblColumns (BoardID,ColumnName,Indx) VALUES (@boardId, @columnName, @indx); ; SELECT last_insert_rowid();";
                            columnOne.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                            insertCommand.Parameters.AddWithValue("@boardId", columnTwo.BoardId);
                            insertCommand.Parameters.AddWithValue("@columnName", columnTwo.ColumnName);
                            insertCommand.Parameters.AddWithValue("@indx", columnTwo.Indx);
                            insertCommand.CommandText =
                                "INSERT INTO tblColumns (BoardID,ColumnName,Indx) VALUES (@boardId, @columnName, @indx); ; SELECT last_insert_rowid();";
                            columnTwo.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                            insertCommand.Parameters.AddWithValue("@boardId", columnThree.BoardId);
                            insertCommand.Parameters.AddWithValue("@columnName", columnThree.ColumnName);
                            insertCommand.Parameters.AddWithValue("@indx", columnThree.Indx);
                            insertCommand.CommandText =
                                "INSERT INTO tblColumns (BoardID,ColumnName,Indx) VALUES (@boardId, @columnName, @indx); ; SELECT last_insert_rowid();";
                            columnThree.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                            insertCommand.Parameters.AddWithValue("@boardId", columnFour.BoardId);
                            insertCommand.Parameters.AddWithValue("@columnName", columnFour.ColumnName);
                            insertCommand.Parameters.AddWithValue("@indx", columnFour.Indx);
                            insertCommand.CommandText =
                                "INSERT INTO tblColumns (BoardID,ColumnName,Indx) VALUES (@boardId, @columnName, @indx); ; SELECT last_insert_rowid();";
                            columnFour.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                            insertCommand.Parameters.AddWithValue("@boardId", columnFive.BoardId);
                            insertCommand.Parameters.AddWithValue("@columnName", columnFive.ColumnName);
                            insertCommand.Parameters.AddWithValue("@indx", columnFive.Indx);
                            insertCommand.CommandText =
                                "INSERT INTO tblColumns (BoardID,ColumnName,Indx) VALUES (@boardId, @columnName, @indx); ; SELECT last_insert_rowid();";
                            columnFive.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                            // Add columns to list
                            columnNames.Add(columnOne);
                            columnNames.Add(columnTwo);
                            columnNames.Add(columnThree);
                            columnNames.Add(columnFour);
                            columnNames.Add(columnFive);
                        }
                    }
                    else // Count != 0
                    {
                        // Get existing columns
                        SqliteCommand selectCommand = new SqliteCommand(
                            "SELECT Id, BoardId, ColumnName, Indx from tblColumns", db);

                        query = selectCommand.ExecuteReader();

                        while (query.Read())
                        {
                            ColumnDTO row = new ColumnDTO
                            {
                                Id = Convert.ToInt32(query.GetString(0)),
                                BoardId = Convert.ToInt32(query.GetString(1)),
                                ColumnName = query.GetString(2),
                                Indx = Convert.ToInt32(query.GetString(3))
                            };
                            columnNames.Add(row);
                        }
                    }

                    return columnNames;
                }

                finally
                {
                    db.Close();
                }
            }
        }
    }
}

