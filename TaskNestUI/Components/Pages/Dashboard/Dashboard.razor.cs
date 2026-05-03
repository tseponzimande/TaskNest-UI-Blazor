namespace TaskNestUI.Components.Pages.Dashboard
{
    public partial class Dashboard
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

        [Inject]
        private Radzen.DialogService _dialogService { get; set; } = null!;

        [Inject]
        private IBoardService _boardService { get; set; } = null!;

        #endregion

        #region Fields

        private bool isLoading = true;
        private string userName = "User";
        private DashboardStatsDto stats = new();
        private List<TasksByStatusDto> tasksByStatus = new();
        private List<TasksByColumnDto> tasksByColumn = new();
        private List<RecentActivityDto> recentActivity = new();
        private List<BoardStatsDto> boardStats = new();
        private List<TaskTrendDto> taskTrend = new();

        #endregion

        #region LifeCycle Methods
        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                _navigationManager.NavigateTo("/login", forceLoad: true);
                return;
            }

            userName = authState.User.Identity.Name ?? "User";
            await LoadDashboardData();
        }
        #endregion

        private async Task LoadDashboardData()
        {
            isLoading = true;
            try
            {
                var statsTask = _dashboardService.GetDashboardStatsAsync();
                var statusTask = _dashboardService.GetTasksByStatusAsync();
                var columnTask = _dashboardService.GetTasksByColumnAsync();
                var activityTask = _dashboardService.GetRecentActivityAsync(10);
                var boardStatsTask = _dashboardService.GetBoardStatsAsync();
                var trendTask = _dashboardService.GetTaskTrendAsync(7);

                await Task.WhenAll(statsTask, statusTask, columnTask, activityTask, boardStatsTask, trendTask);

                stats = await statsTask ?? new DashboardStatsDto();
                tasksByStatus = (await statusTask).ToList();
                tasksByColumn = (await columnTask).ToList();
                recentActivity = (await activityTask).ToList();
                boardStats = (await boardStatsTask).ToList();
                taskTrend = (await trendTask).ToList();

                _notificationService.Notify(NotificationSeverity.Success, "Dashboard", "Data loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load dashboard data");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private string GetActivityIcon(string action) => action switch
        {
            "Created" => "add_circle",
            "Updated" => "edit",
            "Moved" => "swap_horiz",
            "Deleted" => "delete",
            _ => "notifications"
        };

        private string GetActivityColor(string action) => action switch
        {
            "Created" => "#4caf50",
            "Updated" => "#2196f3",
            "Moved" => "#ff9800",
            "Deleted" => "#f44336",
            _ => "#666"
        };

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

        private async Task OpenCreateBoardDialog()
        {
            try
            {
                var result = await _dialogService.OpenAsync<CreateBoardDialog>(
                    "Create New Board",
                    new Dictionary<string, object>(),
                    new Radzen.DialogOptions()
                    {
                        Width = "520px",
                        Height = "auto",
                        Resizable = false,
                        CloseDialogOnEsc = true
                    });

                if (result is BoardDto createModel)
                {
                    var created = await _boardService.CreateBoardAsync(createModel);
                    if (created is not null)
                    {
                        _notificationService.Notify(NotificationSeverity.Success, "Board", "Board created successfully");
                        await LoadDashboardData();
                    }
                    else
                    {
                        _notificationService.Notify(NotificationSeverity.Error, "Board", "Failed to create board");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening create board dialog: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private void GoToMyTasks()
        {
            _navigationManager.NavigateTo("/my-tasks");
        }

    }
}
