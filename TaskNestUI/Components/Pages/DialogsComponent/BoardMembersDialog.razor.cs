namespace TaskNestUI.Components.Pages.DialogsComponent
{
    public partial class BoardMembersDialog
    {
        #region Dependencies

        [Inject]
        private IBoardUserService _boardUserService { get; set; } = null!;

        [Inject]
        private DialogService _dialogService { get; set; } = null!;

        [Inject]
        private NotificationService _notificationService { get; set; } = null!;

        [Inject]
        private ILogger<BoardUserService> _logger { get; set; } = null!;

        #endregion

        #region Parameter

        [Parameter]
        public Guid BoardId { get; set; }

        #endregion

        #region Fields and Properties

        private List<BoardUserDto> boardUsers = new();
        private bool isLoading = true;

        private List<BoardUserRole> roles = new()
        {
            BoardUserRole.Viewer,
            BoardUserRole.Editor,
            BoardUserRole.Admin
        };


        #endregion

        #region Life Cycle Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadBoardUsers();
        }

        #endregion

        #region LoadBoardUsers

        private async Task LoadBoardUsers()
        {
            isLoading = true;
            try
            {
                var users = await _boardUserService.GetBoardUsersAsync(BoardId);
                boardUsers = users.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading board users: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load board members");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        #endregion


        #region OpenAddUserDialog

        private async Task OpenAddUserDialog()
        {
            try
            {
                var result = await _dialogService.OpenAsync<AddUserToBoardDialog>(
                    "Add User to Board",
                    new Dictionary<string, object> { { "BoardId", BoardId } },
                    new Radzen.DialogOptions() { Width = "500px", Height = "auto", Resizable = false });

                if (result is BoardUserDto newUser)
                {
                    await LoadBoardUsers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        #endregion

        #region UpdateRole

        private async Task UpdateRole(BoardUserDto user, object newRole)
        {
            try
            {
                if (newRole is BoardUserRole role && role != user.Role)
                {
                    var success = await _boardUserService.UpdateUserRoleAsync(BoardId, user.ApplicationUserId.ToString(), role);


                    if (success)
                    {
                        user.Role = role;
                        _notificationService.Notify(NotificationSeverity.Success, "Success", "User role updated");
                        await LoadBoardUsers();
                    }
                    else
                    {
                        _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update user role");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating role: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion


        #region ConfirmRemoveUser

        private async Task ConfirmRemoveUser(BoardUserDto user)
        {
            try
            {
                var confirmed = await _dialogService.Confirm(
                    $"Remove {user.UserEmail} from this board?",
                    "Confirm Remove",
                    new ConfirmOptions() { OkButtonText = "Yes, Remove", CancelButtonText = "Cancel" });

                if (confirmed == true)
                {
                    await RemoveUser(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region RemoveUser
        private async Task RemoveUser(BoardUserDto user)
        {
            try
            {
                var success = await _boardUserService.RemoveUserFromBoardAsync(BoardId, user.ApplicationUserId.ToString());

                if (success)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Success", "User removed from board");
                    await LoadBoardUsers();
                }
                else
                {
                    _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to remove user");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing user: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }
        #endregion

        #region IsLastAdmin

        private bool IsLastAdmin(BoardUserDto user)
        {
            return user.Role == BoardUserRole.Admin && boardUsers.Count(u => u.Role == BoardUserRole.Admin) == 1;
        }
        #endregion

        #region Close()

        private void Close()
        {
            _dialogService.Close();
        }

        #endregion
    }
}