namespace TaskNestUI.DTOs
{
    public class UserManagementDtos
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public bool IsEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int BoardCount { get; set; }
    }

    public class ToggleUserStatusDto
    {
        public bool IsEnabled { get; set; }
    }

    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }
}