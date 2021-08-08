using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace KanbanTasker.Services.MSSQL
{
    public class BoardServices: BaseService, IBoardServices
    {
        public BoardServices(Db db, IServiceManifest serviceManifest) :base(db, serviceManifest) { }

        public virtual List<BoardDTO> GetBoards() => db.Boards.Include(x => x.Tasks).ToList();

        public virtual RowOpResult<BoardDTO> SaveBoard(BoardDTO board)
        {
            RowOpResult<BoardDTO> result = new RowOpResult<BoardDTO>(board);

            ValidateBoard(result);

            if (!result.Success)
                return result;

            db.Entry(board).State = board.Id == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();
            result.Success = true;
            return result;
        }
        
        public virtual RowOpResult DeleteBoard(int boardId)
        {
            RowOpResult result = new RowOpResult();
            BoardDTO board = db.Boards.FirstOrDefault(x => x.Id == boardId);

            if (board == null)
            {
                result.ErrorMessage = $"boardId {boardId} is invalid.  Board may have already been deleted.";
                return result;
            }

            db.Entry(board).State = EntityState.Deleted;
            db.SaveChanges();
            result.Success = true;
            return result;
        }

        public List<ColumnDTO> GetColumns(int boardId)
        {
            throw new NotImplementedException();
        }

        public RowOpResult<ColumnDTO> SaveColumn(ColumnDTO column)
        {
            throw new NotImplementedException();
        }

        public RowOpResult CreateColumns(int boardId)
        {
            throw new NotImplementedException();
        }
    }
}

