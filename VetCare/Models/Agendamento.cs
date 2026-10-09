using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Agendamento
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    [Required, StringLength(30)]
    public string Status { get; set; } = "Agendado";
    [StringLength(500)]
    public string? Observacoes { get; set; }
    public int AnimalId { get; set; }
    public Animal? Animal { get; set; }
    public int VeterinarioId { get; set; }
    public Veterinario? Veterinario { get; set; }
    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }
    public Atendimento? Atendimento { get; set; }
}
