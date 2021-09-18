using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using Microsoft.EntityFrameworkCore;
using KanbanTasker.Model.Services;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.MSSQL
{
    public class BoardServices: BaseService, IBoardServices
    {
        public BoardServices(Db db, IServiceManifest serviceManifest) :base(db, serviceManifest) { }

        public virtual List<BoardDto> GetBoards() => db.Boards.Include(x => x.Tasks).ToList();

        public virtual RowOpResult<BoardDto> SaveBoard(BoardDto board)
        {
            RowOpResult<BoardDto> result = new RowOpResult<BoardDto>(board);

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
            BoardDto board = db.Boards.FirstOrDefault(x => x.Id == boardId);

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

        public List<ColumnDto> GetColumns(int boardId)
        {
            throw new NotImplementedException();
        }

        public RowOpResult<ColumnDto> SaveColumn(ColumnDto column)
        {
            throw new NotImplementedException();
        }

        public RowOpResult CreateColumns(int boardId)
        {
            throw new NotImplementedException();
        }

        public ColumnDto CreateColumn(ColumnDto column)
        {
            throw new NotImplementedException();
        }

        public ColumnDto DeleteColumn(ColumnDto column)
        {
            throw new NotImplementedException();
        }

        RowOpResult IBoardServices.DeleteColumn(ColumnDto column)
        {
            throw new NotImplementedException();
        }

        public RowOpResult UpdateColumnIndex(ColumnDto column)
        {
            throw new NotImplementedException();
        }
    }
}

