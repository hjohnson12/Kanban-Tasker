using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public interface IBoardServices
    {
        RowOpResult<BoardDTO> SaveBoard(BoardDTO board);
        RowOpResult DeleteBoard(int boardId);
        List<BoardDTO> GetBoards();
        RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result);
    }
}
