using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;
using VetCare.ViewModels;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
public class AgendamentosController(VetCareDbContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.Agendamentos.Include(x => x.Animal).ThenInclude(x => x!.Tutor).Include(x => x.Veterinario)
            .Include(x => x.Servico).OrderByDescending(x => x.DataHora).ToListAsync());

    public async Task<IActionResult> Create()
    {
        var vm = new AgendamentoViewModel();
        await Fill(vm); return View(vm);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgendamentoViewModel vm)
    {
        await Fill(vm);
        if (vm.DataHora < DateTime.Now) ModelState.AddModelError(nameof(vm.DataHora), "O agendamento deve estar no futuro.");
        var conflito = await db.Agendamentos.AnyAsync(x => x.VeterinarioId == vm.VeterinarioId && x.DataHora == vm.DataHora && x.Status != "Cancelado");
        if (conflito) ModelState.AddModelError(nameof(vm.DataHora), "Este veterinário já possui agendamento neste horário.");
        if (!ModelState.IsValid) return View(vm);
        var item = new Agendamento { DataHora = vm.DataHora, AnimalId = vm.AnimalId, VeterinarioId = vm.VeterinarioId, ServicoId = vm.ServicoId, Status = "Agendado", Observacoes = vm.Observacoes };
        db.Agendamentos.Add(item); await db.SaveChangesAsync();
        TempData["Sucesso"] = "Agendamento criado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(int id, string status)
    {
        var permitidos = new[] { "Agendado", "Confirmado", "Concluído", "Cancelado" };
        if (!permitidos.Contains(status)) return BadRequest();
        var item = await db.Agendamentos.FindAsync(id);
        if (item is null) return NotFound();
        item.Status = status; await db.SaveChangesAsync();
        TempData["Sucesso"] = "Status atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task Fill(AgendamentoViewModel vm)
    {
        vm.Animais = await db.Animais.Include(x => x.Tutor).OrderBy(x => x.Nome)
            .Select(x => new SelectListItem($"{x.Nome} — {x.Tutor!.Nome}", x.Id.ToString())).ToListAsync();
        vm.Veterinarios = await db.Veterinarios.OrderBy(x => x.Nome).Select(x => new SelectListItem(x.Nome, x.Id.ToString())).ToListAsync();
        vm.Servicos = await db.Servicos.Where(x => x.Ativo).OrderBy(x => x.Nome).Select(x => new SelectListItem(x.Nome, x.Id.ToString())).ToListAsync();
    }
}
