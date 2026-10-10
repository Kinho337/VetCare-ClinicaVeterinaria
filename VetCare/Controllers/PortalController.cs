using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;
using VetCare.ViewModels;

namespace VetCare.Controllers;

[Authorize(Roles = "Cliente")]
public class PortalController(VetCareDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tutor = await GetTutorAsync();
        if (tutor is null) return Forbid();

        var animalIds = await db.AnimalTutores.Where(x => x.TutorId == tutor.Id).Select(x => x.AnimalId).ToListAsync();
        var model = new PortalViewModel
        {
            Tutor = tutor,
            Animais = await db.AnimalTutores.Where(x => x.TutorId == tutor.Id)
                .Include(x => x.Animal)
                .Select(x => x.Animal).OrderBy(x => x.Nome).ToListAsync(),
            Agendamentos = await db.Agendamentos.Where(x => animalIds.Contains(x.AnimalId))
                .Include(x => x.Animal).Include(x => x.Servico).Include(x => x.Veterinario)
                .OrderByDescending(x => x.DataHora).Take(20).ToListAsync(),
            Atendimentos = await db.Atendimentos.Where(x => animalIds.Contains(x.AnimalId))
                .Include(x => x.Animal).Include(x => x.Veterinario)
                .OrderByDescending(x => x.DataHora).Take(20).ToListAsync(),
            Solicitacoes = await db.SolicitacoesVinculoAnimal.Where(x => x.TutorId == tutor.Id)
                .Include(x => x.Animal).OrderByDescending(x => x.CriadoEm).Take(10).ToListAsync()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarVinculo(string codigo, string? mensagem)
    {
        var tutor = await GetTutorAsync();
        if (tutor is null) return Forbid();
        codigo = (codigo ?? "").Trim().ToUpperInvariant();
        if (codigo.Length < 6)
        {
            TempData["Erro"] = "Informe o código de vínculo fornecido pela clínica.";
            return RedirectToAction(nameof(Index));
        }

        var animal = await db.Animais.FirstOrDefaultAsync(x => x.CodigoVinculo == codigo);
        if (animal is null)
        {
            TempData["Erro"] = "Não encontramos um pet com esse código. Confira os caracteres ou fale com a clínica.";
            return RedirectToAction(nameof(Index));
        }
        if (await db.AnimalTutores.AnyAsync(x => x.AnimalId == animal.Id && x.TutorId == tutor.Id))
        {
            TempData["Erro"] = "Este pet já está vinculado à sua conta.";
            return RedirectToAction(nameof(Index));
        }
        if (await db.SolicitacoesVinculoAnimal.AnyAsync(x => x.AnimalId == animal.Id && x.TutorId == tutor.Id && x.Status == "Pendente"))
        {
            TempData["Erro"] = "Você já tem uma solicitação pendente para este pet.";
            return RedirectToAction(nameof(Index));
        }

        db.SolicitacoesVinculoAnimal.Add(new SolicitacaoVinculoAnimal
        {
            AnimalId = animal.Id,
            TutorId = tutor.Id,
            Mensagem = string.IsNullOrWhiteSpace(mensagem) ? null : mensagem.Trim(),
            Status = "Pendente"
        });
        await db.SaveChangesAsync();
        TempData["Sucesso"] = "Solicitação enviada. A clínica precisa confirmar o vínculo para proteger os dados do animal.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Tutor?> GetTutorAsync()
    {
        var idClaim = User.FindFirstValue("TutorId");
        if (int.TryParse(idClaim, out var tutorId)) return await db.Tutores.FirstOrDefaultAsync(x => x.Id == tutorId);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var id)) return null;
        var user = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        return user?.TutorId is int linkedTutorId ? await db.Tutores.FirstOrDefaultAsync(x => x.Id == linkedTutorId) : null;
    }
}
