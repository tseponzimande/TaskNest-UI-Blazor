var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRadzenServices()
    .AddLocalStorageServices()
    .AddAuthServices() 
    .AddHttpClientServices(builder.Configuration)
    .AddTaskNestCustomServices()
    .AddSignalRService()
    .AddRazorComponentsServices();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
