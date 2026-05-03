namespace TaskNestUI.Components.Pages.DialogsComponent.TaskStatistics
{
    public partial class TaskStatistics
    {
        #region Parameters

        [Parameter]
        public IEnumerable<TaskItemDto> Tasks { get; set; } = Enumerable.Empty<TaskItemDto>();

        [Parameter]
        public IEnumerable<BoardColumnDto> Columns { get; set; } = Enumerable.Empty<BoardColumnDto>();

        #endregion

        #region Fields and Properties

        private int TotalTasks => Tasks.Count();

        private int OverdueTasks => Tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date);

        private int DueSoonTasks => Tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date >= DateTime.Now.Date && t.DueDate.Value.Date <= DateTime.Now.AddDays(7).Date);

        #endregion

        #region Properties

        private int CompletedTasks
        {
            get
            {
                var doneColumn = Columns.FirstOrDefault(c => c.Name.Contains("Done", StringComparison.OrdinalIgnoreCase) || c.Name.Contains("Complete", StringComparison.OrdinalIgnoreCase));

                return doneColumn != null ? Tasks.Count(t => t.ColumnId == doneColumn.Id) : 0;
            }
        }

        #endregion
    }
}
