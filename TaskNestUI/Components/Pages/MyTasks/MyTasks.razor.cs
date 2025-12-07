namespace TaskNestUI.Components.Pages.MyTasks
{
    public partial class MyTasks
    {
        #region Dependencies

        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        private IBoardService BoardService { get; set; } = null!;

        [Inject]
        private ITaskItemService TaskItemService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        #endregion

        #region Fields

        private bool isLoading = true;
        private List<TaskItemDto> tasks = new();
        private string? search;
        private string? selectedDue;

        private readonly List<string> dueFilters = new()
        {
            "Overdue", "Today", "This Week", "This Month", "No Due Date"
        };

        #endregion

        #region LifeCycle Methods

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                NavigationManager.NavigateTo("/login", forceLoad: true);
                return;
            }
            await LoadData();
        }

        #endregion

        #region LoadData

        private async Task LoadData()
        {
            isLoading = true;
            try
            {
                tasks.Clear();
                var boards = await BoardService.GetBoardAsyc();
                foreach (var b in boards)
                {
                    var boardTasks = await TaskItemService.GetTasksByBoardIdAsync(b.Id);
                    if (boardTasks is not null)
                        tasks.AddRange(boardTasks);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading my tasks: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load tasks");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        #endregion

        #region FilteredTasks

        private IEnumerable<TaskItemDto> FilteredTasks
        {
            get
            {
                IEnumerable<TaskItemDto> result = tasks;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.ToLower();
                    result = result.Where(t => t.Title.ToLower().Contains(s) || (t.Description?.ToLower().Contains(s) ?? false));
                }

                if (!string.IsNullOrWhiteSpace(selectedDue))
                {
                    var now = DateTime.UtcNow.Date;
                    result = selectedDue switch
                    {
                        "Overdue" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < now),
                        "Today" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == now),
                        "This Week" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= now && t.DueDate.Value.Date <= now.AddDays(7)),
                        "This Month" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Month == now.Month && t.DueDate.Value.Year == now.Year),
                        "No Due Date" => result.Where(t => !t.DueDate.HasValue),
                        _ => result
                    };
                }

                return result.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ThenBy(t => t.Title);
            }
        }

        #endregion

        #region OpenBoard
        private void OpenBoard(Guid boardId)
        {
            NavigationManager.NavigateTo($"/boards/{boardId}");
        }

        #endregion
    }
}
