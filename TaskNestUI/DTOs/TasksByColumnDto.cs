namespace TaskNestUI.DTOs
{
    public class TasksByColumnDto
    {
        public string ColumnName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public string BoardName { get; set; } = string.Empty;
    }
}
