using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using LeaderAnalytics.AdaptiveClient;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using KanbanTasker.Model.Services;

namespace KanbanTasker.Services.WebAPI
{
    public class BoardServices: BaseService, IBoardServices
    {
        public BoardServices(Func<IEndPointConfiguration> endPointFactory) : base(endPointFactory) { }

        public List<BoardDTO> GetBoards() => Task.Run(() => Get<List<BoardDTO>>("Boards/GetBoards")).Result;

        public RowOpResult<BoardDTO> SaveBoard(BoardDTO board) => Task.Run(() => Post<BoardDTO, RowOpResult<BoardDTO>>("Boards/SaveBoard", board)).Result;
       
        public RowOpResult DeleteBoard(int boardID) => Task.Run(() => Post<int, RowOpResult>("Boards/DeleteBoard", boardID)).Result;

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

        public ColumnDTO CreateColumn(ColumnDTO column)
        {
            throw new NotImplementedException();
        }
    }
}

