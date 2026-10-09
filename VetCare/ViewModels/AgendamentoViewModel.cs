using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VetCare.ViewModels;

public class AgendamentoViewModel
{
    public int Id { get; set; }
    [Required]
    public DateTime DataHora { get; set; } = DateTime.Now.AddDays(1);
    [Required]
    public int AnimalId { get; set; }
    [Required]
    public int VeterinarioId { get; set; }
    [Required]
    public int ServicoId { get; set; }
    public string Status { get; set; } = "Agendado";
    public string? Observacoes { get; set; }
    public IEnumerable<SelectListItem> Animais { get; set; } = [];
    public IEnumerable<SelectListItem> Veterinarios { get; set; } = [];
    public IEnumerable<SelectListItem> Servicos { get; set; } = [];
}
