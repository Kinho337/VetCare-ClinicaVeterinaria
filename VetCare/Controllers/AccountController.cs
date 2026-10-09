using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;
using VetCare.ViewModels;

namespace VetCare.Controllers;

public class AccountController(VetCareDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.Usuarios.FirstOrDefaultAsync(x => x.Email == model.Email.Trim().ToLower());
        if (user is null || new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, model.Senha) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Perfil)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
