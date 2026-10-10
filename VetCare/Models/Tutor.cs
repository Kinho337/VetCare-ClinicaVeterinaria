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

    // Relação principal antiga, mantida para compatibilidade.
    public ICollection<Animal> Animais { get; set; } = new List<Animal>();
    // Relação N:N: um tutor pode acompanhar vários animais e um animal pode ter vários tutores.
    public ICollection<AnimalTutor> AnimalVinculos { get; set; } = new List<AnimalTutor>();
    public ICollection<SolicitacaoVinculoAnimal> SolicitacoesVinculo { get; set; } = new List<SolicitacaoVinculoAnimal>();
}
