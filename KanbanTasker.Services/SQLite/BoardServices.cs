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

                    deleteCommand = new SqliteCommand
                        ("DELETE FROM tblColumns WHERE BoardID=@boardId", db);
                    deleteCommand.Parameters.AddWithValue("@boardId", boardId);
                    deleteCommand.ExecuteNonQuery();

                    result.Success = true;
                }
                finally
                {
                    db.Close();
                }
                return result;
            }
        }

        public RowOpResult CreateColumns(int boardId)
        {
            RowOpResult result = new RowOpResult();

            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    //List<ColumnDTO> defaultColumns = CreateDefaultColumns(boardId);

                    // Set default columns for new board
                    ColumnDTO columnOne, columnTwo, columnThree, columnFour, columnFive;
                    columnOne = new ColumnDTO
                    {
                        BoardId = boardId,
                        ColumnName = "Backlog",
                        Position = 0,
                        MaxTaskLimit = 10
                    };

                    columnTwo = new ColumnDTO
                    {
                        BoardId = boardId,
                        ColumnName = "To Do",
                        Position = 1,
                        MaxTaskLimit = 10
                    };

                    columnThree = new ColumnDTO
                    {
                        BoardId = boardId,
                        ColumnName = "In Progress",
                        Position = 2,
                        MaxTaskLimit = 10
                    };

                    columnFour = new ColumnDTO
                    {
                        BoardId = boardId,
                        ColumnName = "Review",
                        Position = 3,
                        MaxTaskLimit = 10
                    };

                    columnFive = new ColumnDTO
                    {
                        BoardId = boardId,
                        ColumnName = "Completed",
                        Position = 4,
                        MaxTaskLimit = 10
                    };

                    // Insert columns and return id
                    SqliteCommand insertCommand = new SqliteCommand { Connection = db };
                    insertCommand.Parameters.AddWithValue("@boardId", columnOne.BoardId);
                    insertCommand.Parameters.AddWithValue("@columnName", columnOne.ColumnName);
                    insertCommand.Parameters.AddWithValue("@position", columnOne.Position);
                    insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                    insertCommand.CommandText =
                        "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                        "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                    columnOne.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@boardId", columnTwo.BoardId);
                    insertCommand.Parameters.AddWithValue("@columnName", columnTwo.ColumnName);
                    insertCommand.Parameters.AddWithValue("@position", columnTwo.Position);
                    insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                    insertCommand.CommandText =
                        "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                        "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                    columnTwo.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@boardId", columnThree.BoardId);
                    insertCommand.Parameters.AddWithValue("@columnName", columnThree.ColumnName);
                    insertCommand.Parameters.AddWithValue("@position", columnThree.Position);
                    insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                    insertCommand.CommandText =
                        "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                        "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                    columnThree.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@boardId", columnFour.BoardId);
                    insertCommand.Parameters.AddWithValue("@columnName", columnFour.ColumnName);
                    insertCommand.Parameters.AddWithValue("@position", columnFour.Position);
                    insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                    insertCommand.CommandText =
                        "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                        "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                    columnFour.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@boardId", columnFive.BoardId);
                    insertCommand.Parameters.AddWithValue("@columnName", columnFive.ColumnName);
                    insertCommand.Parameters.AddWithValue("@position", columnFive.Position);
                    insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                    insertCommand.CommandText =
                        "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                        "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                    columnFive.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                    result.Success = true;
                    return result;
                }

                finally
                {
                    db.Close();
                }
            }
        }

        /// <summary>
        /// Returns a list of existing columns or default columns if none
        /// exist already.
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        public List<ColumnDTO> GetColumns(int boardId)
        {
            List<ColumnDTO> columnNames = new List<ColumnDTO>();

            // Get column names from db
            using (SqliteConnection db =
                new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                db.Open();

                try
                {
                    SqliteCommand selectCommand1 = new SqliteCommand { Connection = db };
                    selectCommand1.Parameters.AddWithValue("@boardId", boardId);
                    selectCommand1.CommandText = "SELECT count(*) from tblColumns WHERE BoardID=@boardId";

                    SqliteDataReader query = selectCommand1.ExecuteReader();

                    int count = 0;
                    while (query.Read())
                    {
                        count = Convert.ToInt32(query.GetString(0));
                    }

                    ColumnDTO columnOne, columnTwo, columnThree, columnFour, columnFive;

                    if (count == 0)
                    {
                        // Create default columns for existing boards
                        columnOne = new ColumnDTO
                        {
                            BoardId = boardId,
                            ColumnName = "Backlog",
                            Position = 0,
                            MaxTaskLimit = 10
                        };

                        columnTwo = new ColumnDTO
                        {
                            BoardId = boardId,
                            ColumnName = "To Do",
                            Position = 1,
                            MaxTaskLimit = 10
                        };

                        columnThree = new ColumnDTO
                        {
                            BoardId = boardId,
                            ColumnName = "In Progress",
                            Position = 2,
                            MaxTaskLimit = 10
                        };

                        columnFour = new ColumnDTO
                        {
                            BoardId = boardId,
                            ColumnName = "Review",
                            Position = 3,
                            MaxTaskLimit = 10
                        };

                        columnFive = new ColumnDTO
                        {
                            BoardId = boardId,
                            ColumnName = "Completed",
                            Position = 4,
                            MaxTaskLimit = 10
                        };

                        // Insert columns and return id
                        SqliteCommand insertCommand = new SqliteCommand { Connection = db };
                        insertCommand.Parameters.AddWithValue("@boardId", columnOne.BoardId);
                        insertCommand.Parameters.AddWithValue("@columnName", columnOne.ColumnName);
                        insertCommand.Parameters.AddWithValue("@position", columnOne.Position);
                        insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                        insertCommand.CommandText =
                            "INSERT INTO tblColumns (BoardID,ColumnName,Position,MaxTaskLimit) " +
                            "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                        columnOne.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@boardId", columnTwo.BoardId);
                        insertCommand.Parameters.AddWithValue("@columnName", columnTwo.ColumnName);
                        insertCommand.Parameters.AddWithValue("@position", columnTwo.Position);
                        insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                        insertCommand.CommandText =
                            "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                            "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                        columnTwo.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@boardId", columnThree.BoardId);
                        insertCommand.Parameters.AddWithValue("@columnName", columnThree.ColumnName);
                        insertCommand.Parameters.AddWithValue("@position", columnThree.Position);
                        insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                        insertCommand.CommandText =
                            "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                            "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                        columnThree.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@boardId", columnFour.BoardId);
                        insertCommand.Parameters.AddWithValue("@columnName", columnFour.ColumnName);
                        insertCommand.Parameters.AddWithValue("@position", columnFour.Position);
                        insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                        insertCommand.CommandText =
                            "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                            "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                        columnFour.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@boardId", columnFive.BoardId);
                        insertCommand.Parameters.AddWithValue("@columnName", columnFive.ColumnName);
                        insertCommand.Parameters.AddWithValue("@position", columnFive.Position);
                        insertCommand.Parameters.AddWithValue("@maxTaskLimit", columnOne.MaxTaskLimit);
                        insertCommand.CommandText =
                            "INSERT INTO tblColumns (BoardID,ColumnName,Position, MaxTaskLimit) " +
                            "VALUES (@boardId, @columnName, @position, @maxTaskLimit); ; SELECT last_insert_rowid();";
                        columnFive.Id = Convert.ToInt32(insertCommand.ExecuteScalar());

                        // Add columns to list
                        columnNames.Add(columnOne);
                        columnNames.Add(columnTwo);
                        columnNames.Add(columnThree);
                        columnNames.Add(columnFour);
                        columnNames.Add(columnFive);
                    }
                    else // Count != 0
                    {
                        // Get existing columns
                        SqliteCommand selectCommand = new SqliteCommand(
                            "SELECT Id, BoardId, ColumnName, Position, MaxTaskLimit " +
                            "from tblColumns " +
                            "WHERE BoardId=@boardId", db);
                        selectCommand.Parameters.AddWithValue("@boardId", boardId);

                        query = selectCommand.ExecuteReader();

                        while (query.Read())
                        {
                            ColumnDTO row = new ColumnDTO
                            {
                                Id = Convert.ToInt32(query.GetString(0)),
                                BoardId = Convert.ToInt32(query.GetString(1)),
                                ColumnName = query.GetString(2),
                                Position = Convert.ToInt32(query.GetString(3)),
                                MaxTaskLimit = Convert.ToInt32(query.GetString(4))
                            };
                            columnNames.Add(row);
                        }
                    }
                }
                finally
                {
                    db.Close();
                }
                return columnNames;
            }
        }

        /// <summary>
        /// Updates the column's data in the database
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public RowOpResult<ColumnDTO> SaveColumn(ColumnDTO column)
        {
            RowOpResult<ColumnDTO> result = new RowOpResult<ColumnDTO>(column);

            using (SqliteConnection db = new SqliteConnection(this.db.Database.GetDbConnection().ConnectionString))
            {
                try
                {
                    db.Open();
                    SqliteCommand command = new SqliteCommand { Connection = db };
                    command.Parameters.AddWithValue("@id", column.Id);
                    command.Parameters.AddWithValue("@boardId", column.BoardId);
                    command.Parameters.AddWithValue("@columnName", column.ColumnName);
                    command.Parameters.AddWithValue("@maxTaskLimit", column.MaxTaskLimit);
                    command.CommandText = "UPDATE tblColumns SET ColumnName=@columnName,MaxTaskLimit=@maxTaskLimit WHERE Id=@id AND BoardID=@boardId";
                    command.ExecuteNonQuery();

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
        /// Creates the default columns for a board
        /// </summary>
        internal List<ColumnDTO> CreateDefaultColumns(int boardId)
        {
            List<ColumnDTO> defaultColumns = new List<ColumnDTO>();

            defaultColumns.Add(new ColumnDTO
            {
                BoardId = boardId,
                ColumnName = "Backlog",
                Position = 0,
                MaxTaskLimit = 10
            });

            defaultColumns.Add(new ColumnDTO
            {
                BoardId = boardId,
                ColumnName = "To Do",
                Position = 1,
                MaxTaskLimit = 10
            });

            defaultColumns.Add(new ColumnDTO
            {
                BoardId = boardId,
                ColumnName = "In Progress",
                Position = 2,
                MaxTaskLimit = 10
            });

            defaultColumns.Add(new ColumnDTO
            {
                BoardId = boardId,
                ColumnName = "Review",
                Position = 3,
                MaxTaskLimit = 10
            });

            defaultColumns.Add(new ColumnDTO
            {
                BoardId = boardId,
                ColumnName = "Completed",
                Position = 4,
                MaxTaskLimit = 10
            });

            return defaultColumns;
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

    }
}