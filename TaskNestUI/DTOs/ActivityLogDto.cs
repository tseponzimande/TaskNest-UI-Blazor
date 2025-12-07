namespace TaskNestUI.DTOs
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TaskItemId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
    }
}