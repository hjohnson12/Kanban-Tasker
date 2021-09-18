using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using KanbanTasker.Model.Services;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.MySQL
{
    public class BoardServices: MSSQL.BoardServices, IBoardServices
    {
        public BoardServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest) { }

        public override List<BoardDTO> GetBoards() => base.GetBoards();

        public override RowOpResult<BoardDTO> SaveBoard(BoardDTO board) => base.SaveBoard(board);

        public override RowOpResult DeleteBoard(int boardId) => base.DeleteBoard(boardId);
    }
}

