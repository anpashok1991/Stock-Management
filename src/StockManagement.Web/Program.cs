using MudBlazor.Services;
using Serilog;
using StockManagement.Application;
using StockManagement.Infrastructure;
using StockManagement.Infrastructure.Data;
using StockManagement.Infrastructure.Seed;
using StockManagement.Web.Components;
using StockManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ThemeService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// ---- Seed database ----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<StockManagement.Domain.Entities.Identity.ApplicationUser>>();
    var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<StockManagement.Domain.Entities.Identity.ApplicationRole>>();
    await SeedData.InitializeAsync(context, userManager, roleManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapHealthChecks("/health");
app.MapHub<StockManagement.Infrastructure.Services.AppNotificationHub>("/hubs/notifications");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
