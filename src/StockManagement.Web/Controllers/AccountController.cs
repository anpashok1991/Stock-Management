using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities.Identity;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace StockManagement.Web.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    public record LoginRequest(string Email, string Password, bool RememberMe, string? ReturnUrl);

    [HttpGet("Login")]
    public IActionResult Login()
    {
        // Redirect to the Blazor login page (component at /login)
        return Redirect("/login");
    }

    [HttpPost("Login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Email and password required");

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            // On successful sign-in, redirect the browser to the requested return URL (if local) or root.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            return LocalRedirect("/");
        }

        if (result.IsLockedOut)
        {
            // Redirect back to login with an error indicator (could be handled client-side)
            return Redirect("/login?error=locked");
        }

        return Redirect($"/login?error=invalid");
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/login");
    }

    public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);

    [HttpPost("Register")]
    [Authorize(Roles = "Super Admin")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Email and password required");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName ?? "Admin",
            LastName = model.LastName ?? "User",
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Roles.SuperAdmin);
            if (!string.IsNullOrEmpty(model.Email))
            {
                var tenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault();
                if (tenant != null)
                    user.TenantId = tenant.Id;
            }
            await _context.SaveChangesAsync(default);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Redirect("/");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Redirect($"/register?error={Uri.EscapeDataString(errors)}");
    }
}
