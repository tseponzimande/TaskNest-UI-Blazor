namespace TaskNestUI.DTOs
{
    public class FilterCriteria
    {
        public string? SearchText { get; set; }
        public Guid? ColumnId { get; set; }
        public string? DueDateFilter { get; set; }
    }
}
