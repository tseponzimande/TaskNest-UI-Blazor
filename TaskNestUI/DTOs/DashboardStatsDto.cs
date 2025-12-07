namespace TaskNestUI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalBoards { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int TasksDueThisWeek { get; set; }
        public int ActiveBoards { get; set; }
        public int TotalUsers { get; set; }
        public int MyBoards { get; set; }
        public int MyTasks { get; set; }
    }
}
