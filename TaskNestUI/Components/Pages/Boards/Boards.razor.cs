namespace TaskNestUI.Components.Pages.Boards
{
    public partial class Boards
    {
        #region Dependencies

        [Inject]
        private IBoardService BoardService { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;


        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;


        [Inject]
        private Radzen.DialogService DialogService { get; set; } = null!;


        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        #endregion

        #region Fields

        private IEnumerable<BoardDto> boards = Enumerable.Empty<BoardDto>();
        private bool isLoading = true;

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

            await LoadBoards();
        }

        #endregion

        #region LoadBoards

        private async Task LoadBoards()
        {
            isLoading = true;
            try
            {
                boards = await BoardService.GetBoardAsyc();
                Console.WriteLine($"Loaded {boards.Count()} boards");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading boards: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error Loading Boards", ex.Message);
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        #endregion

        #region OpenBoard

        private void OpenBoard(Guid id)
        {
            NavigationManager.NavigateTo($"/boards/{id}");
        }

        #endregion

        #region OpenCreateDialog

        private async Task OpenCreateDialog()
        {
            try
            {
                var result = await DialogService.OpenAsync<CreateBoardDialog>(
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
                    await CreateBoard(createModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening create dialog: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion

        #region OpenEditDialog

        private async Task OpenEditDialog(BoardDto board)
        {
            try
            {
                var result = await DialogService.OpenAsync<EditBoardDialog>(
                    "Edit Board",
                    new Dictionary<string, object> { { "Board", board } },
                    new Radzen.DialogOptions()
                    {
                        Width = "520px",
                        Height = "auto",
                        Resizable = false,
                        CloseDialogOnEsc = true
                    });

                if (result is BoardDto updatedModel)
                {
                    await UpdateBoard(updatedModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening edit dialog: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion


        #region CreateBoard

        private async Task CreateBoard(BoardDto board)
        {
            try
            {
                var created = await BoardService.CreateBoardAsync(board);

                if (created != null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Board created successfully");
                    await LoadBoards();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to create board");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating board: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion


        #region UpdateBoard

        private async Task UpdateBoard(BoardDto board)
        {
            try
            {
                var updated = await BoardService.UpdateBoardAsync(board);

                if (updated != null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Board updated successfully");
                    await LoadBoards();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update board");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating board: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion


        #region ConfirmDelete

        private async Task ConfirmDelete(BoardDto board)
        {
            try
            {
                var confirmed = await DialogService.Confirm(
                    $"Are you sure you want to delete '{board.Name}'?",
                    "Confirm Delete",
                    new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" }
                );

                if (confirmed == true)
                {
                    await DeleteBoard(board);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in confirm delete: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion


        #region DeleteBoard

        private async Task DeleteBoard(BoardDto board)
        {
            try
            {
                var ok = await BoardService.DeleteBoardAsync(board.Id);

                if (ok)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Board deleted successfully");
                    await LoadBoards();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete board. You may not have permission.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting board: {ex.Message}");
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion
    }
}
