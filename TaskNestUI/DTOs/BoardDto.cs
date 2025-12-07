namespace TaskNestUI.DTOs
{
    public class BoardDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;


        //Navigations
        public List<BoardColumnDto> BoardColumns { get; set; } = new();
    }
}