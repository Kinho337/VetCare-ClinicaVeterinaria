using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
public class AnimaisController(VetCareDbContext db, PhotoService photos) : Controller
{
    public async Task<IActionResult> Index(string? busca) =>
        View(await db.Animais.Include(x => x.Tutor).Include(x => x.TutorVinculos).ThenInclude(x => x.Tutor)
            .Where(x => string.IsNullOrEmpty(busca) || x.Nome.Contains(busca) || x.Tutor!.Nome.Contains(busca) || x.TutorVinculos.Any(v => v.Tutor.Nome.Contains(busca)))
            .OrderBy(x => x.Nome).ToListAsync());

    public async Task<IActionResult> Create(int? tutorId)
    {
        await FillTutors();
        return View(new Animal { TutorId = tutorId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Animal model, IFormFile? foto)
    {
        await FillTutors();
        if (!await db.Tutores.AnyAsync(x => x.Id == model.TutorId)) ModelState.AddModelError(nameof(model.TutorId), "Selecione um tutor válido.");
        if (!string.IsNullOrWhiteSpace(model.Identificacao) && await db.Animais.AnyAsync(x => x.Identificacao == model.Identificacao.Trim()))
            ModelState.AddModelError(nameof(model.Identificacao), "Já existe um animal com essa identificação/microchip. Pesquise a ficha existente para evitar duplicidade.");
        if (!ModelState.IsValid) return View(model);
        try { model.FotoPerfil = await photos.SaveAsync(foto); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); return View(model); }
        model.CodigoVinculo = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.Animais.Add(model);
        await db.SaveChangesAsync();
        db.AnimalTutores.Add(new AnimalTutor { AnimalId = model.Id, TutorId = model.TutorId, TipoVinculo = "Principal" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Sucesso"] = "Animal cadastrado e vinculado ao tutor.";
        return RedirectToAction("Details", "Tutores", new { id = model.TutorId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var animal = await db.Animais.Include(x => x.Tutor)
            .Include(x => x.TutorVinculos).ThenInclude(x => x.Tutor)
            .Include(x => x.Atendimentos).ThenInclude(x => x.Veterinario)
            .FirstOrDefaultAsync(x => x.Id == id);
        return animal is null ? NotFound() : View(animal);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var animal = await db.Animais.FindAsync(id);
        if (animal is null) return NotFound();
        await FillTutors();
        return View(animal);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Animal model, IFormFile? foto)
    {
        if (id != model.Id) return BadRequest();
        await FillTutors();
        var existing = await db.Animais.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) { model.FotoPerfil = existing.FotoPerfil; return View(model); }
        if (!string.IsNullOrWhiteSpace(model.Identificacao) && await db.Animais.AnyAsync(x => x.Id != id && x.Identificacao == model.Identificacao.Trim()))
            ModelState.AddModelError(nameof(model.Identificacao), "Já existe um animal com essa identificação/microchip.");
        if (!ModelState.IsValid) { model.FotoPerfil = existing.FotoPerfil; return View(model); }
        try { model.FotoPerfil = await photos.SaveAsync(foto) ?? existing.FotoPerfil; }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); model.FotoPerfil = existing.FotoPerfil; return View(model); }
        model.CodigoVinculo = existing.CodigoVinculo;
        db.Animais.Update(model);
        if (!await db.AnimalTutores.AnyAsync(x => x.AnimalId == id && x.TutorId == model.TutorId))
            db.AnimalTutores.Add(new AnimalTutor { AnimalId = id, TutorId = model.TutorId, TipoVinculo = "Principal" });
        await db.SaveChangesAsync();
        TempData["Sucesso"] = "Cadastro do animal atualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task FillTutors() => ViewBag.Tutores = new SelectList(await db.Tutores.OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome");
}
