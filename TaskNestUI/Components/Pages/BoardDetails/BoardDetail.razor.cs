namespace TaskNestUI.Components.Pages.BoardDetails
{
    public partial class BoardDetail
    {
        #region Dependencies

        [Inject]
        private IBoardService BoardService { get; set; } = null!;

        [Inject]
        private IBoardColumnService BoardColumnService { get; set; } = null!;

        [Inject]
        private ITaskItemService TaskItemService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        private Radzen.DialogService DialogService { get; set; } = null!;

        [Inject] 
        private SignalRService SignalRService { get; set; } = null!;

        #endregion

        #region Parameters

        [Parameter] public Guid BoardId { get; set; }

        #endregion

        #region Fields

        private BoardDto? board;

        private IEnumerable<BoardColumnDto> columns = Enumerable.Empty<BoardColumnDto>();

        private IEnumerable<TaskItemDto> tasks = Enumerable.Empty<TaskItemDto>();

        private bool isLoading = true;

        private string boardName = "Board";

        private TaskItemDto? draggedTask = null;

        private BoardColumnDto? draggedColumn = null;

        private bool isDraggingTask = false;

        private bool isDraggingColumn = false;

        private string? currentUserId = null;

        private FilterCriteria currentFilter = new();

        private IEnumerable<TaskItemDto> filteredTasks = Enumerable.Empty<TaskItemDto>();

        private HashSet<string> taskBeingDraggedByOthers = new();

        #endregion

        #region Computed Properties
        private IEnumerable<BoardColumnDto> orderedColumns => columns.OrderBy(c => c.Order);

        private string GetTaskStyle(TaskItemDto task)
        {
            var baseStyle = "margin-bottom:0.5rem; cursor:move;";

            if (isDraggingTask && draggedTask?.Id == task.Id)
            {
                baseStyle += " opacity: 0.5;";
            }

            if (taskBeingDraggedByOthers.Contains(task.Id.ToString()))
            {
                baseStyle += " border: 2px dashed #ff9800;";
            }

            return baseStyle;
        }

        private string GetColumnStyle(BoardColumnDto column)
        {
            var baseStyle = "min-width: 300px; max-width: 300px; background: #f8f9fa; border-radius: 8px; padding: 1rem;";

            if (isDraggingColumn && draggedColumn?.Id == column.Id)
            {
                baseStyle += " opacity: 0.5;";
            }

            return baseStyle;
        }
        #endregion

        #region Lifecycle Methods
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                NavigationManager.NavigateTo("/login", forceLoad: true);
                return;
            }

            currentUserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Setup SignalR event handlers
            SignalRService.OnBoardRefreshRequested += HandleBoardRefresh;
            SignalRService.OnTaskDragStarted += HandleTaskDragStarted;
            SignalRService.OnTaskDragEnded += HandleTaskDragEnded;
            SignalRService.OnColumnReordered += HandleColumnReordered;

            // Ensure SignalR is connected before loading board
            await EnsureSignalRConnection();

            await LoadBoardData();
            await SignalRService.JoinBoardGroupAsync(BoardId);
        }

        private async Task EnsureSignalRConnection()
        {
            try
            {
                if (!SignalRService.IsConnected)
                {
                    Console.WriteLine("SignalR not connected, attempting to start...");
                    await SignalRService.StartConnectionAsync();

                  
                    for (int i = 0; i < 10; i++)
                    {
                        if (SignalRService.IsConnected)
                        {
                            Console.WriteLine("SignalR connected successfully");
                            break;
                        }
                        await Task.Delay(100);
                    }
                }

                if (!SignalRService.IsConnected)
                {
                    Console.WriteLine("Warning: SignalR connection could not be established");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring SignalR connection: {ex.Message}");
            }
        }
        #endregion

        #region SignalR Event Handlers
        private async void HandleBoardRefresh()
        {
            await InvokeAsync(async () =>
            {
                await LoadBoardData();
                StateHasChanged();
            });
        }

        private async void HandleTaskDragStarted(string taskId)
        {
            try
            {
                await InvokeAsync(() =>
                {
                    taskBeingDraggedByOthers.Add(taskId);
                    StateHasChanged();
                });
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void HandleTaskDragEnded(string taskId)
        {
            try
            {
                await InvokeAsync(() =>
                {
                    taskBeingDraggedByOthers.Remove(taskId);
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void HandleColumnReordered(object data)
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    await LoadBoardData();
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Data Loading
        private async Task LoadBoardData()
        {
            isLoading = true;
            try
            {
                board = await BoardService.GetBoardByIdAsync(BoardId);
                if (board != null)
                {
                    boardName = board.Name;
                    columns = await BoardColumnService.GetColumnsByBoardIdAsync(BoardId);
                    tasks = await TaskItemService.GetTasksByBoardIdAsync(BoardId);
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading board: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load board data");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private void FilterChange(FilterCriteria criteria)
        {
            currentFilter = criteria;
            ApplyFilters();
            StateHasChanged();
        }

        private void ApplyFilters()
        {
            var result = tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(currentFilter.SearchText))
            {
                var searchLower = currentFilter.SearchText.ToLower();
                result = result.Where(t =>
                    t.Title.ToLower().Contains(searchLower) ||
                    (t.Description?.ToLower().Contains(searchLower) ?? false));
            }

            if (currentFilter.ColumnId.HasValue)
            {
                result = result.Where(t => t.ColumnId == currentFilter.ColumnId.Value);
            }

            if (!string.IsNullOrWhiteSpace(currentFilter.DueDateFilter))
            {
                var now = DateTime.Now;
                result = currentFilter.DueDateFilter switch
                {
                    "Overdue" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < now.Date),
                    "Today" => result.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == now.Date),
                    "This Week" => result.Where(t => t.DueDate.HasValue &&
                        t.DueDate.Value.Date >= now.Date &&
                        t.DueDate.Value.Date <= now.AddDays(7).Date),
                    "This Month" => result.Where(t => t.DueDate.HasValue &&
                        t.DueDate.Value.Month == now.Month &&
                        t.DueDate.Value.Year == now.Year),
                    "No Due Date" => result.Where(t => !t.DueDate.HasValue),
                    _ => result
                };
            }

            filteredTasks = result.ToList();
        }
        #endregion

        #region Task Drag & Drop
        private async Task TaskDragStart(DragEventArgs e, TaskItemDto task)
        {
            draggedTask = task;
            isDraggingTask = true;

            try
            {
                if (e?.DataTransfer != null)
                {
                    e.DataTransfer.EffectAllowed = "move";

                    draggedTask = task;
                    isDraggingTask = true;

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            try
            {
                if (SignalRService.IsConnected && !string.IsNullOrEmpty(currentUserId))
                {
                    await SignalRService.NotifyTaskDragStartAsync(BoardId, task.Id, currentUserId);
                    Console.WriteLine($"Notified drag start for task {task.Id}");
                }
                else
                {
                    Console.WriteLine($"SignalR not connected (IsConnected: {SignalRService.IsConnected}), skipping drag notification");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying task drag start: {ex.Message}");
            }

            StateHasChanged();
        }

        private async Task TaskDragEnd()
        {
            if (draggedTask != null)
            {
                try
                {
                    if (SignalRService.IsConnected)
                    {
                        await SignalRService.NotifyTaskDragEndAsync(BoardId, draggedTask.Id);
                        Console.WriteLine($"Notified drag end for task {draggedTask.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying task drag end: {ex.Message}");
                }
            }

            draggedTask = null;
            isDraggingTask = false;
            StateHasChanged();
        }

        private async Task TaskDrop(DragEventArgs e, Guid targetColumnId)
        {

            Guid draggedId = Guid.Empty;
            try
            {
                if (draggedTask != null)
                    draggedId = draggedTask.Id;

            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }

            if (draggedId == Guid.Empty && draggedTask != null)
            {
                Guid.TryParse(draggedTask.Id.ToString(), out draggedId);
            }

            if (draggedId == Guid.Empty) return;

            var draggedTaskObj = tasks.FirstOrDefault(t => t.Id == draggedId);
            if (draggedTaskObj == null)
            {
                await TaskDragEnd();
                return;
            }

            if (draggedTaskObj.ColumnId == targetColumnId)
            {
                await TaskDragEnd();
                return;
            }

            try
            {
                var targetColumnTasks = tasks.Where(t => t.ColumnId == targetColumnId).OrderBy(t => t.Position).ToList();
                var newPosition = targetColumnTasks.Any() ? targetColumnTasks.Max(t => t.Position) + 1 : 0;

                var success = await TaskItemService.MoveTaskAsync(draggedId, targetColumnId, newPosition);

                if (success)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success",
                        $"Task moved to {columns.FirstOrDefault(c => c.Id == targetColumnId)?.Name}");

 
                    await LoadBoardData();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to move task");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving task: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to move task: {ex.Message}");
            }
            finally
            {
                await TaskDragEnd();
            }
        }
        #endregion

        #region Column Drag & Drop
        private void ColumnDragStart(DragEventArgs e, BoardColumnDto column)
        {
            try
            {
                draggedColumn = column;
                isDraggingColumn = true;

                try
                {
                    if (e?.DataTransfer != null)
                    {
                        e.DataTransfer.EffectAllowed = "move";
                    }

                    draggedColumn = column;
                    isDraggingColumn = true;

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ColumnDragEnd()
        {
            try
            {
                draggedColumn = null;
                isDraggingColumn = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ColumnDragOver(DragEventArgs e)
        {
            try
            {
                if (e?.DataTransfer != null)
                {
                    e.DataTransfer.DropEffect = "move";
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message); 
            }
        }

        private void ColumnDragEnter(DragEventArgs e, BoardColumnDto column)
        {
            StateHasChanged();
        }

        private void ColumnDragLeave(DragEventArgs e, BoardColumnDto column)
        {
            StateHasChanged();
        }


        private async Task ColumnDrop(DragEventArgs e, BoardColumnDto targetColumn)
        {
            if (draggedColumn == null)
            {
                ColumnDragEnd();
                return;
            }

            var draggedId = draggedColumn.Id;

            if (draggedId == targetColumn.Id)
            {
                ColumnDragEnd();
                return;
            }

            try
            {
                var orderedCols = columns.OrderBy(c => c.Order).ToList();
                var draggedIndex = orderedCols.FindIndex(c => c.Id == draggedId);
                var targetIndex = orderedCols.FindIndex(c => c.Id == targetColumn.Id);

                if (draggedIndex == -1 || targetIndex == -1)
                {
                    ColumnDragEnd();
                    return;
                }

                var column = orderedCols[draggedIndex];
                orderedCols.RemoveAt(draggedIndex);
                orderedCols.Insert(targetIndex, column);

                for (int i = 0; i < orderedCols.Count; i++)
                {
                    orderedCols[i].Order = i;
                }

                var columnOrders = orderedCols.Select(c => new ColumnOrderDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    Order = c.Order
                }).ToList();

                var success = await BoardColumnService.ReorderColumnsAsync(columnOrders);

                if (success)
                {
                    columns = orderedCols;
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Columns reordered");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to reorder columns");
                    await LoadBoardData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reordering columns: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to reorder columns");
                await LoadBoardData();
            }
            finally
            {
                ColumnDragEnd();
            }
        }

        #endregion

        #region Dialog Methods
        private async Task OpenAddColumnDialog()
        {
            try
            {
                var result = await DialogService.OpenAsync<CreateColumnDialog>(
                    "Add Column",
                    new Dictionary<string, object> { { "BoardId", BoardId } },
                    new Radzen.DialogOptions() { Width = "450px", Height = "auto", Resizable = false });

                if (result is BoardColumnDto newColumn)
                {
                    await LoadBoardData();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Column added successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task OpenEditColumnDialog(BoardColumnDto column)
        {
            try
            {
                var result = await DialogService.OpenAsync<EditColumnDialog>(
                    "Edit Column",
                    new Dictionary<string, object> { { "Column", column } },
                    new Radzen.DialogOptions() { Width = "450px", Height = "auto", Resizable = false });

                if (result is BoardColumnDto updatedColumn)
                {
                    await LoadBoardData();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Column updated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task ConfirmDeleteColumn(BoardColumnDto column)
        {
            var confirmed = await DialogService.Confirm(
                $"Delete column '{column.Name}'? All tasks in this column will also be deleted.",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                await DeleteColumn(column);
            }
        }

        private async Task DeleteColumn(BoardColumnDto column)
        {
            try
            {
                var success = await BoardColumnService.DeleteColumnAsync(column.Id);
                if (success)
                {
                    await LoadBoardData();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Column deleted successfully");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete column");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task OpenAddTaskDialog(Guid columnId)
        {
            try
            {
                var result = await DialogService.OpenAsync<CreateTaskDialog>(
                    "Add Task",
                    new Dictionary<string, object> { { "BoardId", BoardId }, { "ColumnId", columnId } },
                    new Radzen.DialogOptions() { Width = "550px", Height = "auto", Resizable = false });

                if (result is TaskItemDto newTask)
                {
                    await LoadBoardData();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Task added successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task OpenTaskDetailsDialog(TaskItemDto task)
        {
            try
            {
                var result = await DialogService.OpenAsync<TaskDetailsDialog>(
                    "Task Details",
                    new Dictionary<string, object>
                    {
                        { "Task", task },
                        { "Columns", columns },
                        { "CurrentUserId", currentUserId ?? string.Empty }
                    },
                    new Radzen.DialogOptions() { Width = "700px", Height = "auto", Resizable = false });

                if (result != null)
                {
                    await LoadBoardData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private void ShowTaskMenu(MouseEventArgs args, TaskItemDto task)
        {
            OpenTaskDetailsDialog(task);
        }

        private async Task OpenManageMembersDialog()
        {
            try
            {
                await DialogService.OpenAsync<BoardMembersDialog>(
                    "Manage Board Members",
                    new Dictionary<string, object> { { "BoardId", BoardId } },
                    new Radzen.DialogOptions() { Width = "700px", Height = "auto", Resizable = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task OpenBoardSettings()
        {
            try
            {
                if (board == null) return;

                var result = await DialogService.OpenAsync<BoardSettingsDialog>(
                    "Board Settings",
                    new Dictionary<string, object> { { "Board", board } },
                    new Radzen.DialogOptions() { Width = "520px", Height = "auto", Resizable = false });

                if (result is BoardDto updated)
                {
                    await LoadBoardData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }
        #endregion

        #region Disposal
        public async ValueTask DisposeAsync()
        {
            SignalRService.OnBoardRefreshRequested -= HandleBoardRefresh;
            SignalRService.OnTaskDragStarted -= HandleTaskDragStarted;
            SignalRService.OnTaskDragEnded -= HandleTaskDragEnded;
            SignalRService.OnColumnReordered -= HandleColumnReordered;
            await SignalRService.LeaveBoardGroupAsync(BoardId);
        }
        #endregion
    }
}