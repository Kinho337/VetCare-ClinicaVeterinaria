using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetCare.Data;
using VetCare.Models;

namespace VetCare.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
public class AnimaisController(VetCareDbContext db, PhotoService photos) : Controller
{
    public async Task<IActionResult> Index(string? busca)
    {
        var query = db.Animais
            .AsNoTracking()
            .Include(x => x.Tutor)
            .Include(x => x.TutorVinculos).ThenInclude(x => x.Tutor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(x => x.Nome.Contains(termo)
                || (x.Tutor != null && x.Tutor.Nome.Contains(termo))
                || x.TutorVinculos.Any(v => v.Tutor.Nome.Contains(termo))
                || (x.Identificacao != null && x.Identificacao.Contains(termo)));
        }

        ViewBag.Busca = busca;
        return View(await query.OrderBy(x => x.Nome).ToListAsync());
    }

    public async Task<IActionResult> Create(int? tutorId)
    {
        await FillTutors(tutorId);
        return View(new Animal { TutorId = tutorId ?? 0, Especie = "Cão" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Animal model, IFormFile? foto)
    {
        Normalize(model);
        await FillTutors(model.TutorId > 0 ? model.TutorId : null);

        if (model.TutorId <= 0 || !await db.Tutores.AnyAsync(x => x.Id == model.TutorId))
            ModelState.AddModelError(nameof(model.TutorId), "Selecione um tutor válido para este animal.");

        if (!string.IsNullOrWhiteSpace(model.Identificacao) && await db.Animais.AnyAsync(x => x.Identificacao == model.Identificacao))
            ModelState.AddModelError(nameof(model.Identificacao), "Já existe um animal com essa identificação ou microchip. Pesquise a ficha existente para evitar duplicidade.");

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            model.FotoPerfil = await photos.SaveAsync(foto);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("foto", ex.Message);
            return View(model);
        }

        // Garante que o EF não tente persistir propriedades de navegação enviadas pelo formulário.
        model.Tutor = null;
        model.TutorVinculos = new List<AnimalTutor>();
        model.SolicitacoesVinculo = new List<SolicitacaoVinculoAnimal>();
        model.Atendimentos = new List<Atendimento>();
        model.Agendamentos = new List<Agendamento>();
        model.CodigoVinculo = await GenerateUniqueLinkCodeAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            db.Animais.Add(model);
            await db.SaveChangesAsync();

            // Mantém a coluna TutorId legada e a relação N:N sincronizadas.
            db.AnimalTutores.Add(new AnimalTutor
            {
                AnimalId = model.Id,
                TutorId = model.TutorId,
                TipoVinculo = "Principal"
            });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty,
                "Não foi possível salvar o animal. Confira se o banco está atualizado com as tabelas AnimalTutores e a coluna CodigoVinculo, e se o tutor selecionado ainda existe.");
            return View(model);
        }

        TempData["Sucesso"] = $"{model.Nome} foi cadastrado e vinculado ao tutor. O código de vínculo familiar está disponível na ficha do animal.";
        return RedirectToAction("Details", "Tutores", new { id = model.TutorId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var animal = await db.Animais.AsNoTracking()
            .Include(x => x.Tutor)
            .Include(x => x.TutorVinculos).ThenInclude(x => x.Tutor)
            .Include(x => x.Atendimentos).ThenInclude(x => x.Veterinario)
            .FirstOrDefaultAsync(x => x.Id == id);
        return animal is null ? NotFound() : View(animal);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var animal = await db.Animais.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (animal is null) return NotFound();
        await FillTutors(animal.TutorId);
        return View(animal);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Animal model, IFormFile? foto)
    {
        if (id != model.Id) return BadRequest();
        Normalize(model);

        var existing = await db.Animais.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();
        await FillTutors(model.TutorId > 0 ? model.TutorId : null);

        if (model.TutorId <= 0 || !await db.Tutores.AnyAsync(x => x.Id == model.TutorId))
            ModelState.AddModelError(nameof(model.TutorId), "Selecione um tutor válido.");
        if (!string.IsNullOrWhiteSpace(model.Identificacao) && await db.Animais.AnyAsync(x => x.Id != id && x.Identificacao == model.Identificacao))
            ModelState.AddModelError(nameof(model.Identificacao), "Já existe outro animal com essa identificação ou microchip.");

        if (!ModelState.IsValid)
        {
            model.FotoPerfil = existing.FotoPerfil;
            return View(model);
        }

        try
        {
            var novaFoto = await photos.SaveAsync(foto);
            existing.FotoPerfil = novaFoto ?? existing.FotoPerfil;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("foto", ex.Message);
            model.FotoPerfil = existing.FotoPerfil;
            return View(model);
        }

        // Atualiza apenas os campos editáveis; preserva código, histórico e vínculos familiares existentes.
        existing.Nome = model.Nome;
        existing.Especie = model.Especie;
        existing.Raca = model.Raca;
        existing.Sexo = model.Sexo;
        existing.DataNascimento = model.DataNascimento;
        existing.Identificacao = model.Identificacao;

        // Se o responsável principal mudar, adiciona o novo vínculo sem remover os demais tutores.
        if (existing.TutorId != model.TutorId)
        {
            var previousPrincipal = await db.AnimalTutores
                .FirstOrDefaultAsync(x => x.AnimalId == id && x.TutorId == existing.TutorId);
            if (previousPrincipal is not null && previousPrincipal.TipoVinculo == "Principal")
                previousPrincipal.TipoVinculo = "Tutor";

            existing.TutorId = model.TutorId;
            var newPrincipal = await db.AnimalTutores
                .FirstOrDefaultAsync(x => x.AnimalId == id && x.TutorId == model.TutorId);
            if (newPrincipal is null)
            {
                db.AnimalTutores.Add(new AnimalTutor
                {
                    AnimalId = id,
                    TutorId = model.TutorId,
                    TipoVinculo = "Principal"
                });
            }
            else
            {
                newPrincipal.TipoVinculo = "Principal";
            }
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar a ficha. Confira os dados e a estrutura do banco de dados.");
            model.FotoPerfil = existing.FotoPerfil;
            return View(model);
        }

        TempData["Sucesso"] = "Ficha do animal atualizada. Os demais vínculos familiares foram preservados.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task FillTutors(int? selectedId = null)
    {
        var tutores = await db.Tutores.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        ViewBag.Tutores = new SelectList(tutores, "Id", "Nome", selectedId);
    }

    private async Task<string> GenerateUniqueLinkCodeAsync()
    {
        string code;
        do
        {
            code = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        }
        while (await db.Animais.AnyAsync(x => x.CodigoVinculo == code));
        return code;
    }

    private static void Normalize(Animal model)
    {
        model.Nome = (model.Nome ?? string.Empty).Trim();
        model.Especie = (model.Especie ?? string.Empty).Trim();
        model.Raca = string.IsNullOrWhiteSpace(model.Raca) ? null : model.Raca.Trim();
        model.Sexo = string.IsNullOrWhiteSpace(model.Sexo) ? null : model.Sexo.Trim();
        model.Identificacao = string.IsNullOrWhiteSpace(model.Identificacao) ? null : model.Identificacao.Trim().ToUpperInvariant();
        model.CodigoVinculo = (model.CodigoVinculo ?? string.Empty).Trim();
    }
}
