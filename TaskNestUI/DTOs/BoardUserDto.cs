namespace TaskNestUI.DTOs
{
    public class BoardUserDto
    {
        public Guid Id { get; set; }
        public Guid boardId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public BoardUserRole Role { get; set; }
    }
}