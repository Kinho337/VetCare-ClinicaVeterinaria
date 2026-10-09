using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador")]
public class ServicosController(VetCareDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Servicos.OrderBy(x => x.Nome).ToListAsync());
    public IActionResult Create() => View(new Servico());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Servico model)
    {
        if (!ModelState.IsValid) return View(model);
        db.Servicos.Add(model); await db.SaveChangesAsync();
        TempData["Sucesso"] = "Serviço cadastrado.";
        return RedirectToAction(nameof(Index));
    }
}
