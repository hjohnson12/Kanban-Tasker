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
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.WebAPI
{
    public class BoardServices: BaseService, IBoardServices
    {
        public BoardServices(Func<IEndPointConfiguration> endPointFactory) : base(endPointFactory) { }

        public List<BoardDto> GetBoards() => Task.Run(() => Get<List<BoardDto>>("Boards/GetBoards")).Result;

        public RowOpResult<BoardDto> SaveBoard(BoardDto board) => Task.Run(() => Post<BoardDto, RowOpResult<BoardDto>>("Boards/SaveBoard", board)).Result;
       
        public RowOpResult DeleteBoard(int boardID) => Task.Run(() => Post<int, RowOpResult>("Boards/DeleteBoard", boardID)).Result;

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

