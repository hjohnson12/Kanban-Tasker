using System.Collections.Generic;

namespace KanbanTasker.Model
{
    public interface IKanbanTaskerService
    {
        RowOpResult<BoardDTO> AddBoard(BoardDTO board);
        RowOpResult<TaskDTO> AddTask(TaskDTO task);
        bool DeleteBoard(int boardId);
        bool DeleteTask(int id);
        List<BoardDTO> GetBoards();
        List<TaskDTO> GetTasks();
        RowOpResult<BoardDTO> UpdateBoard(BoardDTO board);
        void UpdateCardIndex(int iD, int currentCardIndex);
        void UpdateColumnData(TaskDTO task);
        RowOpResult<TaskDTO> UpdateTask(TaskDTO task);
        RowOpResult<TaskDTO> ValidateTask(RowOpResult<TaskDTO> result);
        RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result);
    }
}