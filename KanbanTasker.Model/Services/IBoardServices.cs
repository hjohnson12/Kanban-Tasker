using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model.Services
{
    /// <summary>
    /// Interface for a service that interacts with the board in Kanban.
    /// </summary>
    public interface IBoardServices
    {
        RowOpResult<BoardDTO> SaveBoard(BoardDTO board);
        RowOpResult DeleteBoard(int boardId);
        List<BoardDTO> GetBoards();
        RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result);
        RowOpResult CreateColumns(int boardId);
        RowOpResult<ColumnDTO> SaveColumn(ColumnDTO column);
        List<ColumnDTO> GetColumns(int boardId);
        ColumnDTO CreateColumn(ColumnDTO column);
        RowOpResult DeleteColumn(ColumnDTO column);
        RowOpResult UpdateColumnIndex(ColumnDTO column);
    }
}