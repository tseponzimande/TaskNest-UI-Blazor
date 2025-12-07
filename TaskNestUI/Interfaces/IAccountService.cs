namespace TaskNestUI.Interfaces
{
    public interface IAccountService
    {
        Task<string?> LoginAsync(LoginDto model);
        Task<bool> RegisterAsync(RegisterDto model);
        Task<bool> ChangePasswordAsync(ChangePasswordDto model);
        Task<bool> ConfirmEmailAsync(string userId, string token);
    }
}