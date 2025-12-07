var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRadzenServices()
    .AddLocalStorageServices()
    .AddAuthServices() 
    .AddHttpClientServices(builder.Configuration)
    .AddTaskNestCustomServices()
    .AddSignalRService()
    .AddRazorComponentsServices();

// Add Authentication and Authorization services
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
