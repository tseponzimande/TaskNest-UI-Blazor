namespace TaskNestUI.Components
{
    public partial class App
    {
        #region Dependencies

        [Inject]
        private IJSRuntime jSRuntime { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider authenticationStateProvider { get; set; } = null!;

        [Inject]
        private NavigationManager navigationManager { get; set; } = null!;

        #endregion


        #region Life Cycle Methods

        protected override async Task OnInitializedAsync()
        {
            if (authenticationStateProvider is JwtAuthenticationStateProvider jwtAuthenticationStateProvider)
            {
                await jwtAuthenticationStateProvider.InitializeAsync();
            }
        }

        #endregion
    }
}
