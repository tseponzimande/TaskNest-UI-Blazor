namespace TaskNestUI.DTOs
{
    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? BoardName { get; set; }
    }
}
