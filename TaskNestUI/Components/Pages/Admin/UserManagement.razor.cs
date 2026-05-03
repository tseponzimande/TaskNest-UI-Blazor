namespace TaskNestUI.Components.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public partial class UserManagement
    {
        #region Dependencies

        [Inject]
        private IUserManagementService _userManagementService { get; set; } = null!;

        [Inject]
        private NotificationService _notificationService { get; set; } = null!;

        [Inject]
        private NavigationManager _navigationManager { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; } = null!;

        [Inject]
        private Radzen.DialogService _dialogService { get; set; } = null!;

        [Inject]
        private ILogger<UserManagement> _logger { get; set; } = null!;

        #endregion

        #region Fields

        private List<UserManagementDtos> users = new();

        public bool isLoading = false;

        #endregion

        #region LifeCycle methods

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var isAdmin = authState.User.IsInRole("Admin");

            if (!isAdmin)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Access Denied", "You don't have permission to access this page");
                _navigationManager.NavigateTo("/boards");
                return;
            }

            await LoadUsers();
        }

        #endregion

        #region LoadUsers

        private async Task LoadUsers()
        {
            isLoading = true;
            try
            {
                var result = await _userManagementService.GetAllUsersAsync();
                users = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load users");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        #endregion

        #region ConfirmToggleStatus

        private async Task ConfirmToggleStatus(UserManagementDtos user, bool enable)
        {
            var action = enable ? "enable" : "disable";
            var confirmed = await _dialogService.Confirm(
                $"Are you sure you want to {action} user '{user.Email}'?",
                $"Confirm {(enable ? "Enable" : "Disable")}",
                new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                await ToggleUserStatus(user, enable);
            }
        }

        #endregion

        #region ToggleUserStatus

        private async Task ToggleUserStatus(UserManagementDtos user, bool enable)
        {
            try
            {
                var success = await _userManagementService.ToggleUserStatusAsync(user.Id, enable);

                if (success)
                {
                    user.IsEnabled = enable;
                    _notificationService.Notify(NotificationSeverity.Success, "Success", $"User {(enable ? "enabled" : "disabled")} successfully");
                    StateHasChanged();
                }
                else
                {
                    _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update user status");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling user status: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion

        #region OpenEditRolesDialog

        private async Task OpenEditRolesDialog(UserManagementDtos user)
        {
            try
            {
                var result = await _dialogService.OpenAsync<EditUserRolesDialog>(
                    "Edit User Roles",
                    new Dictionary<string, object> { { "User", user } },
                    new Radzen.DialogOptions() { Width = "500px", Height = "auto", Resizable = false });

                if (result is List<string> updatedRoles)
                {
                    var success = await _userManagementService.UpdateUserRolesAsync(user.Id, updatedRoles);

                    if (success)
                    {
                        user.Roles = updatedRoles;
                        _notificationService.Notify(NotificationSeverity.Success, "Success", "User roles updated successfully");
                        StateHasChanged();
                    }
                    else
                    {
                        _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update user roles");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion

        #region ConfirmDeleteUser

        private async Task ConfirmDeleteUser(UserManagementDtos user)
        {
            var confirmed = await _dialogService.Confirm(
                $"Are you sure you want to delete user '{user.Email}'? This action cannot be undone.",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                await DeleteUser(user);
            }
        }

        #endregion

        #region DeleteUser

        private async Task DeleteUser(UserManagementDtos user)
        {
            try
            {
                var success = await _userManagementService.DeleteUserAsync(user.Id);

                if (success)
                {
                    users.Remove(user);
                    _notificationService.Notify(NotificationSeverity.Success, "Success", "User deleted successfully");
                    StateHasChanged();
                }
                else
                {
                    _notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete user");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                _notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        #endregion

        #region GetRelativeTime

        private string GetRelativeTime(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalMinutes < 1) 
                return "Just now";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes}m ago";

            if (span.TotalHours < 24) 
                return $"{(int)span.TotalHours}h ago";

            if (span.TotalDays < 30) 
                return $"{(int)span.TotalDays}d ago";

            return dateTime.ToString("MMM dd, yyyy");
        }

        #endregion
    }
}
