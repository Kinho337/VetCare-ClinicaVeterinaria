using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Animal
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Nome { get; set; } = "";
    [Required, StringLength(50)]
    public string Especie { get; set; } = "Cão";
    [StringLength(100)]
    public string? Raca { get; set; }
    [StringLength(30)]
    public string? Sexo { get; set; }
    public DateTime? DataNascimento { get; set; }
    [StringLength(80)]
    public string? Identificacao { get; set; }
    [StringLength(500)]
    public string? FotoPerfil { get; set; }
    public int TutorId { get; set; }
    public Tutor? Tutor { get; set; }
    public ICollection<Atendimento> Atendimentos { get; set; } = new List<Atendimento>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
