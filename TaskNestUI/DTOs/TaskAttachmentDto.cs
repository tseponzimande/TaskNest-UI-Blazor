namespace TaskNestUI.DTOs
{
    public class TaskAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid TaskItemId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
    }
}