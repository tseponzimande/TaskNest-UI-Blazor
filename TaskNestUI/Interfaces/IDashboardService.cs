namespace TaskNestUI.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto?> GetDashboardStatsAsync();
        Task<IEnumerable<TasksByStatusDto>> GetTasksByStatusAsync();
        Task<IEnumerable<TasksByColumnDto>> GetTasksByColumnAsync();
        Task<IEnumerable<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
        Task<IEnumerable<BoardStatsDto>> GetBoardStatsAsync();
        Task<IEnumerable<TaskTrendDto>> GetTaskTrendAsync(int days = 7);
    }
}