using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockManagement.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;

namespace StockManagement.Web.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
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

    [HttpPost("Logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/");
    }
}
