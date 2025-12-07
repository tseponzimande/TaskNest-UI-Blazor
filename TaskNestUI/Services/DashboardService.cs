namespace TaskNestUI.Services
{
    public class DashboardService(HttpClient httpClient, ILogger<DashboardService> logger) : IDashboardService
    {
        private readonly HttpClient _httpClient = httpClient;

        private readonly ILogger<DashboardService> _logger = logger;


        public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/dashboard/stats");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching dashboard stats: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<TasksByStatusDto>> GetTasksByStatusAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<TasksByStatusDto>>("api/dashboard/tasks-by-status");
                return data ?? Enumerable.Empty<TasksByStatusDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching tasks by status: {ex.Message}");
                return Enumerable.Empty<TasksByStatusDto>();
            }
        }

        public async Task<IEnumerable<TasksByColumnDto>> GetTasksByColumnAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<TasksByColumnDto>>("api/dashboard/tasks-by-column");
                return data ?? Enumerable.Empty<TasksByColumnDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching tasks by column: {ex.Message}");
                return Enumerable.Empty<TasksByColumnDto>();
            }
        }

        public async Task<IEnumerable<RecentActivityDto>> GetRecentActivityAsync(int count = 10)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<RecentActivityDto>>($"api/dashboard/recent-activity?count={count}");
                return data ?? Enumerable.Empty<RecentActivityDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching recent activity: {ex.Message}");
                return Enumerable.Empty<RecentActivityDto>();
            }
        }

        public async Task<IEnumerable<BoardStatsDto>> GetBoardStatsAsync()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<BoardStatsDto>>("api/dashboard/board-stats");
                return data ?? Enumerable.Empty<BoardStatsDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching board stats: {ex.Message}");
                return Enumerable.Empty<BoardStatsDto>();
            }
        }

        public async Task<IEnumerable<TaskTrendDto>> GetTaskTrendAsync(int days = 7)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<List<TaskTrendDto>>($"api/dashboard/task-trend?days={days}");
                return data ?? Enumerable.Empty<TaskTrendDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching task trend: {ex.Message}");
                return Enumerable.Empty<TaskTrendDto>();
            }
        }
    }
}
