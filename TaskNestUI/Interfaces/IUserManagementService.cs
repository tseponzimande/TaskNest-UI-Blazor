namespace TaskNestUI.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserManagementDtos>> GetAllUsersAsync();
        Task<UserManagementDtos?> GetUserByIdAsync(string userId);
        Task<bool> ToggleUserStatusAsync(string userId, bool isEnabled);
        Task<bool> UpdateUserRolesAsync(string userId, List<string> roles);
        Task<bool> DeleteUserAsync(string userId);
    }
}