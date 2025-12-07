namespace TaskNestUI.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(Guid taskId);
        Task<CommentDto?> CreateCommentAsync(Guid taskId, CreateCommentDto dto);
        Task<bool> DeleteCommentAsync(Guid taskId, Guid commentId);
    }
}
