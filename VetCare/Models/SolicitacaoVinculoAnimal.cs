using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class SolicitacaoVinculoAnimal
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public int TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pendente";
    [StringLength(500)]
    public string? Mensagem { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvidoEm { get; set; }
    [StringLength(180)]
    public string? ResolvidoPor { get; set; }
}
