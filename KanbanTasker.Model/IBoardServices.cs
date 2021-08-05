using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    /// <summary>
    /// Interface for a service that interacts with the board in Kanban.
    /// </summary>
    public interface IBoardServices
    {
        RowOpResult<BoardDTO> SaveBoard(BoardDTO board);
        RowOpResult DeleteBoard(int boardId);
        List<BoardDTO> GetBoards();
        List<ColumnDTO> GetColumnNames(int boardId);
        RowOpResult CreateColumns(int boardId);
        RowOpResult<ColumnDTO> SaveColumn(ColumnDTO column);
        RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result);
    }
}
