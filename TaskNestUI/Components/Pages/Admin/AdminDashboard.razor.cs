namespace TaskNestUI.Components.Pages.Admin
{
    public partial class AdminDashboard
    {
        #region Dependencies

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; } = null!;

        [Inject]
        private NotificationService _notificationService { get; set; } = null!;

        [Inject]
        private NavigationManager _navigationManager { get; set; } = null!;

        [Inject]
        private IDashboardService _dashboardService { get; set; } = null!;

        #endregion

        #region Fields

        private bool isLoading = true;
        private DashboardStatsDto stats = new();
        private List<TasksByStatusDto> tasksByStatus = new();
        private List<RecentActivityDto> recentActivity = new();

        #endregion

        #region LifeCycle Methods

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var isAdmin = authState.User.IsInRole("Admin");

            if (!isAdmin)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Access Denied",
                    "You don't have permission to access this page");
                _navigationManager.NavigateTo("/dashboard");
                return;
            }

            await LoadDashboardData();
        }

        #endregion

        #region Methods

        private async Task LoadDashboardData()
        {
            isLoading = true;
            try
            {
                stats = await _dashboardService.GetDashboardStatsAsync() ?? new();
                tasksByStatus = (await _dashboardService.GetTasksByStatusAsync()).ToList();
                recentActivity = (await _dashboardService.GetRecentActivityAsync(10)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading admin dashboard: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error",
                    "Failed to load dashboard data");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private string GetRelativeTime(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalMinutes < 1)
                return "Just now";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes}m ago";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours}h ago";

            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays}d ago";

            return dateTime.ToString("MMM dd");
        }

        #endregion
    }
}