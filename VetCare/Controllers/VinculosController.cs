using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
public class VinculosController(VetCareDbContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.SolicitacoesVinculoAnimal.Include(x => x.Animal).Include(x => x.Tutor)
            .OrderBy(x => x.Status == "Pendente" ? 0 : 1).ThenByDescending(x => x.CriadoEm).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int id)
    {
        var solicitacao = await db.SolicitacoesVinculoAnimal.Include(x => x.Animal).Include(x => x.Tutor).FirstOrDefaultAsync(x => x.Id == id);
        if (solicitacao is null) return NotFound();
        if (solicitacao.Status != "Pendente") { TempData["Erro"] = "Esta solicitação já foi analisada."; return RedirectToAction(nameof(Index)); }

        if (!await db.AnimalTutores.AnyAsync(x => x.AnimalId == solicitacao.AnimalId && x.TutorId == solicitacao.TutorId))
            db.AnimalTutores.Add(new AnimalTutor { AnimalId = solicitacao.AnimalId, TutorId = solicitacao.TutorId, TipoVinculo = "Familiar" });

        solicitacao.Status = "Aprovada";
        solicitacao.ResolvidoEm = DateTime.UtcNow;
        solicitacao.ResolvidoPor = User.Identity?.Name;
        await db.SaveChangesAsync();
        TempData["Sucesso"] = $"Vínculo de {solicitacao.Tutor.Nome} com {solicitacao.Animal.Nome} aprovado. A ficha do animal foi preservada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Recusar(int id)
    {
        var solicitacao = await db.SolicitacoesVinculoAnimal.FindAsync(id);
        if (solicitacao is null) return NotFound();
        if (solicitacao.Status == "Pendente")
        {
            solicitacao.Status = "Recusada";
            solicitacao.ResolvidoEm = DateTime.UtcNow;
            solicitacao.ResolvidoPor = User.Identity?.Name;
            await db.SaveChangesAsync();
        }
        TempData["Sucesso"] = "Solicitação atualizada.";
        return RedirectToAction(nameof(Index));
    }
}
