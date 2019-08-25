using System.Collections.Generic;

namespace KanbanTasker.Model
{
    public interface IKanbanTaskerService
    {
        int AddBoard(BoardDTO board);
        (int, bool) AddTask(TaskDTO task);
        bool DeleteBoard(int boardId);
        bool DeleteTask(int id);
        List<BoardDTO> GetBoards();
        List<TaskDTO> GetTasks();
        bool UpdateBoard(BoardDTO board);
        void UpdateCardIndex(int iD, int currentCardIndex);
        void UpdateColumnData(TaskDTO task);
        bool UpdateTask(TaskDTO task);
    }
}