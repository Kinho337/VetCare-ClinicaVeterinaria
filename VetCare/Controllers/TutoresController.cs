using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
public class TutoresController(VetCareDbContext db, PhotoService photos) : Controller
{
    public async Task<IActionResult> Index(string? busca) =>
        View(await db.Tutores.Include(x => x.Animais).Include(x => x.AnimalVinculos).Where(x => string.IsNullOrEmpty(busca) || x.Nome.Contains(busca) || x.CPF.Contains(busca) || x.Email.Contains(busca)).OrderBy(x => x.Nome).ToListAsync());

    public IActionResult Create() => View(new Tutor());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tutor model, IFormFile? foto)
    {
        model.CPF = Digits(model.CPF);
        model.Email = model.Email.Trim().ToLowerInvariant();
        if (await db.Tutores.AnyAsync(x => x.CPF == model.CPF)) ModelState.AddModelError(nameof(model.CPF), "Já existe um tutor cadastrado com este CPF.");
        if (await db.Tutores.AnyAsync(x => x.Email == model.Email)) ModelState.AddModelError(nameof(model.Email), "Já existe um tutor cadastrado com este e-mail.");
        if (!ModelState.IsValid) return View(model);
        try { model.FotoPerfil = await photos.SaveAsync(foto); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); return View(model); }
        db.Tutores.Add(model); await db.SaveChangesAsync();
        TempData["Sucesso"] = "Tutor cadastrado com sucesso.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var tutor = await db.Tutores.Include(x => x.Animais).ThenInclude(x => x.Agendamentos)
            .Include(x => x.AnimalVinculos).ThenInclude(x => x.Animal)
            .FirstOrDefaultAsync(x => x.Id == id);
        return tutor is null ? NotFound() : View(tutor);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tutor = await db.Tutores.FindAsync(id);
        return tutor is null ? NotFound() : View(tutor);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tutor model, IFormFile? foto)
    {
        if (id != model.Id) return BadRequest();
        model.CPF = Digits(model.CPF);
        model.Email = model.Email.Trim().ToLowerInvariant();
        if (await db.Tutores.AnyAsync(x => x.Id != id && x.CPF == model.CPF)) ModelState.AddModelError(nameof(model.CPF), "CPF já utilizado.");
        if (await db.Tutores.AnyAsync(x => x.Id != id && x.Email == model.Email)) ModelState.AddModelError(nameof(model.Email), "E-mail já utilizado.");
        var existing = await db.Tutores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) { model.FotoPerfil = existing.FotoPerfil; return View(model); }
        try { model.FotoPerfil = await photos.SaveAsync(foto) ?? existing.FotoPerfil; }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); model.FotoPerfil = existing.FotoPerfil; return View(model); }
        db.Tutores.Update(model); await db.SaveChangesAsync();
        TempData["Sucesso"] = "Cadastro atualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private static string Digits(string value) => new(value.Where(char.IsDigit).ToArray());
}
