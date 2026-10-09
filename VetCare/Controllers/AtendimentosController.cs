using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario")]
public class AtendimentosController(VetCareDbContext db, PhotoService photos) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.Atendimentos.Include(x => x.Animal).ThenInclude(x => x!.Tutor).Include(x => x.Veterinario)
            .OrderByDescending(x => x.DataHora).ToListAsync());

    public async Task<IActionResult> Create(int? agendamentoId)
    {
        var model = new Atendimento { AgendamentoId = agendamentoId, DataHora = DateTime.Now };
        if (agendamentoId.HasValue)
        {
            var ag = await db.Agendamentos.FindAsync(agendamentoId.Value);
            if (ag is not null) { model.AnimalId = ag.AnimalId; model.VeterinarioId = ag.VeterinarioId; }
        }
        await Fill(model); return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atendimento model, IFormFile? foto)
    {
        await Fill(model);
        if (model.AgendamentoId.HasValue && await db.Atendimentos.AnyAsync(x => x.AgendamentoId == model.AgendamentoId))
            ModelState.AddModelError(nameof(model.AgendamentoId), "Este agendamento já possui atendimento registrado.");
        if (!ModelState.IsValid) return View(model);
        try { model.FotoAtendimento = await photos.SaveAsync(foto); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("foto", ex.Message); return View(model); }
        db.Atendimentos.Add(model);
        if (model.AgendamentoId.HasValue)
        {
            var ag = await db.Agendamentos.FindAsync(model.AgendamentoId.Value);
            if (ag is not null) ag.Status = "Concluído";
        }
        await db.SaveChangesAsync();
        TempData["Sucesso"] = "Atendimento registrado.";
        return RedirectToAction(nameof(Report), new { id = model.Id });
    }

    public async Task<IActionResult> Report(int id)
    {
        var item = await db.Atendimentos.Include(x => x.Animal).ThenInclude(x => x!.Tutor)
            .Include(x => x.Veterinario).Include(x => x.Agendamento).ThenInclude(x => x!.Servico)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    private async Task Fill(Atendimento model)
    {
        ViewBag.Animais = new SelectList(await db.Animais.Include(x => x.Tutor).OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", model.AnimalId);
        ViewBag.Veterinarios = new SelectList(await db.Veterinarios.OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", model.VeterinarioId);
        ViewBag.Agendamentos = new SelectList(await db.Agendamentos.Where(x => x.Status != "Cancelado").OrderByDescending(x => x.DataHora).ToListAsync(), "Id", "Id", model.AgendamentoId);
    }
}
