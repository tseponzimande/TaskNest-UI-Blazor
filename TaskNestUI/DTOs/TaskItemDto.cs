namespace TaskNestUI.DTOs
{
    public class TaskItemDto
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public Guid? ColumnId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int Position { get; set; }
    }
}