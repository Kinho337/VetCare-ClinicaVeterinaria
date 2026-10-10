using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class AnimalTutor
{
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public int TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;
    public DateTime VinculadoEm { get; set; } = DateTime.UtcNow;
    [StringLength(30)]
    public string TipoVinculo { get; set; } = "Tutor";
}
