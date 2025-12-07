namespace TaskNestUI.Components.Pages.Accounts
{
    public partial class ChangePassword
    {
        #region Dependencies

        [Inject]
        public IAccountService AccountService { get; set; } = null!;

        [Inject]
        public NotificationService NotificationService { get; set; } = null!;

        [Inject]
        public ILogger<ChangePassword> _logger { get; set; } = null!;

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        #endregion

        #region Fields and Properties

        private ChangePasswordDto changeModel = new();

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                NavigationManager.NavigateTo("/login", forceLoad: true);
            }
        }

        private async Task OnChangePassword(ChangePasswordDto model)
        {
            try
            {
                if (changeModel.NewPassword != changeModel.ConfirmNewPassword)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Passwords do not match.");
                    return;
                }
                var success = await AccountService.ChangePasswordAsync(changeModel);
                if (success)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Password changed successfully!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Password change failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured {ex.Message}", ex);
            }
        }

        #endregion
    }
}
