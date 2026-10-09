using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario")]
public class VeterinariosController(VetCareDbContext db, PhotoService photos) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Veterinarios.OrderBy(x => x.Nome).ToListAsync());
    public IActionResult Create() => View(new Veterinario());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Veterinario model, IFormFile? foto)
    {
        model.Email = model.Email.Trim().ToLowerInvariant();
        if (await db.Veterinarios.AnyAsync(x => x.Email == model.Email)) ModelState.AddModelError(nameof(model.Email), "E-mail já cadastrado.");
        if (await db.Veterinarios.AnyAsync(x => x.CRmv == model.CRmv)) ModelState.AddModelError(nameof(model.CRmv), "CRMV já cadastrado.");
        if (!ModelState.IsValid) return View(model);
        try { model.FotoPerfil = await photos.SaveAsync(foto); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); return View(model); }
        db.Veterinarios.Add(model); await db.SaveChangesAsync();
        TempData["Sucesso"] = "Veterinário cadastrado.";
        return RedirectToAction(nameof(Index));
    }
}
