namespace TaskNestUI.Components.Pages.Accounts
{
    public partial class Login
    {
        #region Dependencies

        [Inject]
        private IAccountService AccountService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        #endregion

        #region Fields and Properties

        private LoginDto loginModel = new();
        private bool isLoading = false;

        #endregion

        #region lifeCycle Methods
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated ?? false)
            {
                NavigationManager.NavigateTo("/boards", forceLoad: true);
            }
        }
        #endregion

        #region UserLogin
        private async Task UserLogin(LoginDto model)
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                var token = await AccountService.LoginAsync(loginModel);
                if (!string.IsNullOrEmpty(token))
                {
                    if (AuthStateProvider is JwtAuthenticationStateProvider jwtProvider)
                    {
                        await jwtProvider.Login(token);
                        await Task.Delay(200);

                        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                        var isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;

                        if (isAuthenticated)
                        {
                            var isAdmin = authState.User?.IsInRole("Admin") ?? false;
                            var redirectPath = isAdmin ? "/admin/dashboard" : "/dashboard";

                            NotificationService.Notify(new NotificationMessage
                            {
                                Severity = NotificationSeverity.Success,
                                Summary = "Login Successful",
                                Detail = $"Welcome {(isAdmin ? "Admin" : "")}!",
                                Duration = 4000
                            });

                            NavigationManager.NavigateTo(redirectPath, forceLoad: true);
                        }
                    }
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Login Failed",
                        Detail = "Invalid username or password.",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error occurred: {ex.Message}",
                    Duration = 4000
                });
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
