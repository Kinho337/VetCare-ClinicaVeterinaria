using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;

namespace VetCare.Controllers;

[Authorize]
public class HomeController(VetCareDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Tutores = await db.Tutores.CountAsync();
        ViewBag.Animais = await db.Animais.CountAsync();
        ViewBag.Agendamentos = await db.Agendamentos.CountAsync(x => x.DataHora.Date == DateTime.Today && x.Status != "Cancelado");
        ViewBag.Atendimentos = await db.Atendimentos.CountAsync();
        ViewBag.Proximos = await db.Agendamentos.Include(x => x.Animal).ThenInclude(x => x!.Tutor)
            .Include(x => x.Veterinario).Include(x => x.Servico)
            .Where(x => x.DataHora >= DateTime.Now && x.Status != "Cancelado")
            .OrderBy(x => x.DataHora).Take(6).ToListAsync();
        return View();
    }
    public IActionResult Error() => View();
}
