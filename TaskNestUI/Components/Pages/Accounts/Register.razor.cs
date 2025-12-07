namespace TaskNestUI.Components.Pages.Accounts
{
    public partial class Register
    {
        #region Dependencies

        [Inject]
        public IAccountService AccountService { get; set; } = null!;

        [Inject]
        public NavigationManager navigationManager { get; set; } = null!;

        [Inject]
        public NotificationService NotificationService { get; set; } = null!;

        [Inject]
        public ILogger<Register> Logger { get; set; } = null!;

        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        #endregion

        #region Fields and Properties

        private RegisterDto registerModel = new();
        private bool isLoading = false;
        private string errorMessage = string.Empty;

        #endregion

        #region LifeCycle Methods

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated ?? false)
            {
                navigationManager.NavigateTo("/boards", forceLoad: true);
            }
        }
        #endregion

        #region UserRegister
        public async Task UserRegister(RegisterDto model)
        {
            isLoading = true;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                Logger.LogInformation("Starting registration process for: {Email}", registerModel.Email);

                var success = await AccountService.RegisterAsync(registerModel);

                if (success)
                {
                    Logger.LogInformation("Registration successful for: {Email}", registerModel.Email);
                    NotificationService.Notify(NotificationSeverity.Success, "Registration successful! Please login.");
                    navigationManager.NavigateTo("/login");
                }
                else
                {
                    Logger.LogWarning("Registration failed for: {Email}", registerModel.Email);
                    errorMessage = "Registration failed. Please check your details and try again.";
                    NotificationService.Notify(NotificationSeverity.Error, "Registration failed.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during registration for: {Email}", registerModel.Email);
                errorMessage = $"Registration error: {ex.Message}";
                NotificationService.Notify(NotificationSeverity.Error, "Registration error occurred.");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
        #endregion
    }
}
