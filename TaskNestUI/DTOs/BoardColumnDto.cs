namespace TaskNestUI.DTOs
{
    public class BoardColumnDto
    {
        public Guid Id { get; set; }

        public Guid BoardId { get; set; }

        public string Name { get; set; } = null!;

        public int Order { get; set; }
    }
}