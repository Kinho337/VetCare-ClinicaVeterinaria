using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Tutor
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    public string Nome { get; set; } = "";
    [Required, StringLength(14, MinimumLength = 11)]
    public string CPF { get; set; } = "";
    [Required, EmailAddress, StringLength(180)]
    public string Email { get; set; } = "";
    [Required, StringLength(30)]
    public string Telefone { get; set; } = "";
    [StringLength(180)]
    public string? Endereco { get; set; }
    [StringLength(500)]
    public string? FotoPerfil { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public ICollection<Animal> Animais { get; set; } = new List<Animal>();
}
