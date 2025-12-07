namespace TaskNestUI.Components.Pages.Accounts
{
    public partial class ConfirmEmail
    {
        #region Dependencies

        [Inject] 
        public IAccountService AccountService { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject] 
        public NotificationService NotificationService { get; set; } = null!;

        [Inject]
        public ILogger _logger {  get; set; } = null!;

        #endregion

        #region Parameters

        [Parameter]
        [SupplyParameterFromQuery(Name = "userId")] 
        public string? UserId { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "token")]
        public string? Token { get; set; }

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Token))
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Invalid confirmation link.");
                    NavigationManager.NavigateTo("/login");
                    return;
                }

                var success = await AccountService.ConfirmEmailAsync(UserId, Token);
                if (success)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Email confirmed! You can now login.");
                    NavigationManager.NavigateTo("/login");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Email confirmation failed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occured: {ex.Message}");
            }
        }

        #endregion
    }
}