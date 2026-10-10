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
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null || new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, model.Senha) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }
        await SignInUser(user);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return user.Perfil == "Cliente" ? RedirectToAction("Index", "Portal") : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        model.CPF = new string((model.CPF ?? "").Where(char.IsDigit).ToArray());
        if (model.CPF.Length != 11)
            ModelState.AddModelError(nameof(model.CPF), "Informe um CPF com 11 números.");
        if (await db.Usuarios.AnyAsync(x => x.Email == model.Email))
            ModelState.AddModelError(nameof(model.Email), "Já existe uma conta com este e-mail.");

        var tutorPorCpf = await db.Tutores.FirstOrDefaultAsync(x => x.CPF == model.CPF);
        var tutorPorEmail = await db.Tutores.FirstOrDefaultAsync(x => x.Email == model.Email);
        Tutor? tutorExistente = tutorPorCpf ?? tutorPorEmail;
        if (tutorPorCpf is not null && tutorPorEmail is not null && tutorPorCpf.Id != tutorPorEmail.Id)
            ModelState.AddModelError("", "O CPF e o e-mail pertencem a cadastros diferentes. Entre em contato com a clínica.");
        else if (tutorExistente is not null &&
                 (tutorExistente.CPF != model.CPF || !string.Equals(tutorExistente.Email, model.Email, StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError("", "Já existe um cadastro na clínica com parte desses dados. Confira CPF e e-mail ou fale com a equipe.");
        if (!ModelState.IsValid) return View(model);

        await using var transaction = await db.Database.BeginTransactionAsync();
        var tutor = tutorExistente;
        if (tutor is null)
        {
            tutor = new Tutor { Nome = model.Nome.Trim(), CPF = model.CPF, Email = model.Email, Telefone = model.Telefone.Trim(), Endereco = model.Endereco?.Trim() };
            db.Tutores.Add(tutor);
            await db.SaveChangesAsync();
        }

        var user = new AppUser { Nome = tutor.Nome, Email = tutor.Email, Perfil = "Cliente", TutorId = tutor.Id };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, model.Senha);
        db.Usuarios.Add(user);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        await SignInUser(user);
        TempData["Sucesso"] = "Conta criada! Agora você pode acompanhar os cuidados do seu pet.";
        return RedirectToAction("Index", "Portal");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private async Task SignInUser(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Perfil)
        };
        if (user.TutorId.HasValue) claims.Add(new Claim("TutorId", user.TutorId.Value.ToString()));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}
