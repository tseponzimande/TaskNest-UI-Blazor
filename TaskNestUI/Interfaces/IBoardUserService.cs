namespace TaskNestUI.Interfaces
{
    public interface IBoardUserService
    {
        Task<IEnumerable<BoardUserDto>> GetBoardUsersAsync(Guid boardId);
        Task<BoardUserDto?> AddUserToBoardAsync(Guid boardId, AddBoardUserDto dto);
        Task<bool> UpdateUserRoleAsync(Guid boardId, string userId, BoardUserRole role);
        Task<bool> RemoveUserFromBoardAsync(Guid boardId, string userId);
    }
}
