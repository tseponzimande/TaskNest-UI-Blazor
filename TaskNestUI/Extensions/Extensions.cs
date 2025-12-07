namespace TaskNestUI.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddRadzenServices(this IServiceCollection services)
        {
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();
            services.AddScoped<ThemeService>();

            return services;
        }

        public static IServiceCollection AddLocalStorageServices(this IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
            return services;
        }


        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<JwtAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<JwtAuthenticationStateProvider>());

            services.AddCascadingAuthenticationState();

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration config)
        {
            var apiBase = config["TaskNestApiBase"] ?? "https://localhost:7179/";

            services.AddScoped<AuthHttpMessageHandler>();

            services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(apiBase);
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHttpMessageHandler>();

            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("API");
            });

            return services;
        }

        public static IServiceCollection AddTaskNestCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IBoardService, BoardService>();
            services.AddScoped<IBoardColumnService, BoardColumnService>();
            services.AddScoped<ITaskItemService, TaskItemService>();
            services.AddScoped<IBoardUserService, BoardUserService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();

            return services;
        }


        public static IServiceCollection AddSignalRService(this IServiceCollection services)
        {
            services.AddScoped<SignalRService>();
            return services;
        }

        public static IServiceCollection AddRazorComponentsServices(this IServiceCollection services)
        {
            services.AddRazorComponents()
                    .AddInteractiveServerComponents();
            return services;
        }

        public static WebApplication UseTaskNestPipeline(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            // Add these two lines
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            return app;
        }
    }
}