namespace TaskNestUI.Interfaces
{
    public interface ITaskItemService
    {
        Task<IEnumerable<TaskItemDto>> GetTasksByBoardIdAsync(Guid boardId);
        Task<IEnumerable<TaskItemDto>> GetTasksByColumnIdAsync(Guid columnId);
        Task<TaskItemDto?> GetTaskByIdAsync(Guid id);
        Task<TaskItemDto?> CreateTaskAsync(TaskItemDto dto);
        Task<TaskItemDto?> UpdateTaskAsync(TaskItemDto dto);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<bool> MoveTaskAsync(Guid taskId, Guid newColumnId, int newPosition);
    }
}