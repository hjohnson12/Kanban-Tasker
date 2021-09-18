using KanbanTasker.Model.Dto;
using System;
using System.Collections.Generic;

namespace KanbanTasker.Model.Services
{
    /// <summary>
    /// Interface for a service that interacts with the board in Kanban.
    /// </summary>
    public interface IBoardServices
    {
        RowOpResult<BoardDto> SaveBoard(BoardDto board);
        RowOpResult DeleteBoard(int boardId);
        List<BoardDto> GetBoards();
        RowOpResult<BoardDto> ValidateBoard(RowOpResult<BoardDto> result);
        RowOpResult CreateColumns(int boardId);
        RowOpResult<ColumnDto> SaveColumn(ColumnDto column);
        List<ColumnDto> GetColumns(int boardId);
        ColumnDto CreateColumn(ColumnDto column);
        RowOpResult DeleteColumn(ColumnDto column);
        RowOpResult UpdateColumnIndex(ColumnDto column);
    }
}