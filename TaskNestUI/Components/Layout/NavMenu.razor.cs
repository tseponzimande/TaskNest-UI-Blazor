namespace TaskNestUI.Components.Layout
{
    public partial class NavMenu
    {
        #region Dependencies

        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private NotificationService NotificationService { get; set; } = null!;

        #endregion

        #region Fields

        private bool isAuthenticated = false;
        private bool isAdmin = false;
        private bool isInitialized = false;
        private bool multiple = true;
        private string dashboardPath = "/dashboard";

        #endregion

        #region LifeCycle Methods

        protected override async Task OnInitializedAsync()
        {
            AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;

            if (AuthStateProvider is JwtAuthenticationStateProvider jwtProvider)
            {
                await jwtProvider.InitializeAsync();
            }

            await CheckAuthenticationState();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (AuthStateProvider is JwtAuthenticationStateProvider jwtProvider && !isInitialized)
                {
                    await jwtProvider.InitializeAsync();
                    isInitialized = true;
                }

                await CheckAuthenticationState();
                StateHasChanged();
            }
        }

        #endregion

        #region CheckAuthenticationState

        private async Task CheckAuthenticationState()
        {
            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var wasAuthenticated = isAuthenticated;
                var wasAdmin = isAdmin;

                isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
                isAdmin = authState.User?.IsInRole("Admin") ?? false;

                // Set dashboard path based on role
                dashboardPath = isAdmin ? "/admin/dashboard" : "/dashboard";

                if (wasAuthenticated != isAuthenticated || wasAdmin != isAdmin)
                {
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking auth state: {ex.Message}");
                isAuthenticated = false;
                isAdmin = false;
            }
        }

        #endregion

        #region OnAuthenticationStateChanged

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await CheckAuthenticationState();
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Logout

        private async Task Logout()
        {
            try
            {
                var jwtProvider = (JwtAuthenticationStateProvider)AuthStateProvider;
                await jwtProvider.Logout();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Logged Out",
                    Detail = "You have been logged out successfully.",
                    Duration = 4000
                });

                isAuthenticated = false;
                isAdmin = false;
                StateHasChanged();

                await Task.Delay(100);
                NavigationManager.NavigateTo("/login", forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        #endregion
    }
}
