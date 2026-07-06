using MudBlazor.Services;
using Serilog;
using StockManagement.Application;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Infrastructure;
using StockManagement.Infrastructure.Data;
using StockManagement.Infrastructure.Seed;
using StockManagement.Web.Components;
using StockManagement.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    // Needed because Blazor Server SSR-to-interactive transition creates
    // a brief overlap of two MudPopoverProvider instances, which throws
    // by default and terminates the circuit.
    config.PopoverOptions.ThrowOnDuplicateProvider = false;
});
// Register CircuitTenantState for both HTTP requests and Blazor circuits
// (TenantInitializer sets the tenant on first use)
builder.Services.AddScoped<CircuitTenantState>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<CircuitTenantState>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ThemeService>();

// Enable detailed Blazor circuit errors
builder.Services.Configure<CircuitOptions>(options => { options.DetailedErrors = true; });
builder.Services.AddScoped<CircuitHandler, CircuitErrorHandler>();
// Tenant initializer for Blazor server circuits
builder.Services.AddScoped<StockManagement.Web.Services.TenantInitializer>();

// Antiforgery
builder.Services.AddAntiforgery();

// Register HttpClient for Blazor Server components so relative URLs work
builder.Services.AddScoped<HttpClient>(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

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
else
{
    // Show developer exception page in Development so detailed stack traces appear in the console/browser
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Set the AppDbContext.CurrentTenantId for each request so query filters use the correct tenant
app.Use(async (context, next) =>
{
    var tenant = context.RequestServices.GetService<StockManagement.Application.Common.Interfaces.ITenantContext>();
    var db = context.RequestServices.GetService<StockManagement.Infrastructure.Data.AppDbContext>();
    if (db != null)
    {
        db.CurrentTenantId = tenant?.TenantId ?? Guid.Empty;
        // If tenant not provided, fallback to the first seeded tenant so UI flows work in dev
        if (db.CurrentTenantId == Guid.Empty)
        {
            try
            {
                var defaultTenantId = await db.Tenants.IgnoreQueryFilters().Select(t => t.Id).FirstOrDefaultAsync();
                if (defaultTenantId != Guid.Empty)
                    db.CurrentTenantId = defaultTenantId;
            }
            catch
            {
                // ignore failures here (e.g., migrations not applied yet)
            }
        }
    }
    await next();
});

app.MapHealthChecks("/health");
app.MapHub<StockManagement.Infrastructure.Services.AppNotificationHub>("/hubs/notifications");
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
