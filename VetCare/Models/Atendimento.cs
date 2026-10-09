using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Atendimento
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    [Required, StringLength(2000)]
    public string Queixa { get; set; } = "";
    [Required, StringLength(4000)]
    public string Diagnostico { get; set; } = "";
    [StringLength(4000)]
    public string? Observacoes { get; set; }
    [StringLength(4000)]
    public string? MedicacoesRecomendadas { get; set; }
    [StringLength(500)]
    public string? FotoAtendimento { get; set; }
    public int AnimalId { get; set; }
    public Animal? Animal { get; set; }
    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }
    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
}
