namespace TaskNestUI.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid TaskItemId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
    }
}