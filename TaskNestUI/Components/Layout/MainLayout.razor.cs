namespace TaskNestUI.Components.Layout
{
    public partial class MainLayout
    {
        #region Dependencies

        [Inject]
        private NavigationManager Nav { get; set; } = null!;

        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        private SignalRService SignalRService { get; set; } = null!;

        #endregion

        #region Fields

        private bool sidebarExpanded = true;
        private bool isAuthenticated = false;

        #endregion

        #region LifeCycle Methods

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (AuthStateProvider is JwtAuthenticationStateProvider jwtProvider)
                {
                    await jwtProvider.InitializeAsync();
                }

                await CheckAuthentication();


                if (isAuthenticated)
                {
                    await InitializeSignalR();
                }

                StateHasChanged();
            }
        }

        private async Task InitializeSignalR()
        {
            try
            {
                if (!SignalRService.IsConnected)
                {
                    Console.WriteLine("Initializing SignalR connection from MainLayout...");
                    await SignalRService.StartConnectionAsync();

                    await Task.Delay(500);

                    if (SignalRService.IsConnected)
                    {
                        Console.WriteLine("SignalR connected successfully in MainLayout");
                    }
                    else
                    {
                        Console.WriteLine("SignalR connection not established in MainLayout");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SignalR in MainLayout: {ex.Message}");
            }
        }
        #endregion

        #region ToggleSidebar

        private void ToggleSidebar()
        {
            sidebarExpanded = !sidebarExpanded;
        }

        #endregion

        #region CheckAuthentication

        private async Task CheckAuthentication()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
        }

        #endregion
    }
}


//namespace TaskNestUI.Components.Layout
//{
//    public partial class MainLayout
//    {
//        #region Fields

//        private bool sidebarExpanded = true;
//        private bool isAuthenticated = false;

//        #endregion

//        #region Dependencies

//        [Inject] 
//        private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

//        [Inject]
//        private SignalRService SignalRService { get; set; } = null!;

//        #endregion

//        #region LifeCycle Methods

//        protected override async Task OnAfterRenderAsync(bool firstRender)
//        {
//            if (firstRender)
//            {
//                if (AuthStateProvider is JwtAuthenticationStateProvider jwtProvider)
//                {
//                    await jwtProvider.InitializeAsync();
//                }

//                await CheckAuthentication();


//                if (isAuthenticated)
//                {
//                    await InitializeSignalR();
//                }

//                StateHasChanged();
//            }
//        }

//        private async Task InitializeSignalR()
//        {
//            try
//            {
//                if (!SignalRService.IsConnected)
//                {
//                    Console.WriteLine("Initializing SignalR connection from MainLayout...");
//                    await SignalRService.StartConnectionAsync();

//                    // Wait a bit for connection
//                    await Task.Delay(500);

//                    if (SignalRService.IsConnected)
//                    {
//                        Console.WriteLine("SignalR connected successfully in MainLayout");
//                    }
//                    else
//                    {
//                        Console.WriteLine("SignalR connection not established in MainLayout");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error initializing SignalR in MainLayout: {ex.Message}");
//            }
//        }
//        #endregion

//        #region ToggleSidebar

//        private void ToggleSidebar()
//        {
//            sidebarExpanded = !sidebarExpanded;
//        }

//        #endregion

//        #region CheckAuthentication

//        private async Task CheckAuthentication()
//        {
//            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
//            isAuthenticated = authState.User?.Identity?.IsAuthenticated ?? false;
//        }

//        #endregion
//    }
//}
