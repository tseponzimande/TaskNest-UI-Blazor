namespace TaskNestUI.DTOs
{
    public class BoardStatsDto
    {
        public Guid BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int CompletedTasks { get; set; }
        public int MemberCount { get; set; }
    }
}