using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Veterinario
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(180)]
    public string Email { get; set; } = "";
    [Required, StringLength(30)]
    public string CRMV { get; set; } = "";
    [StringLength(30)]
    public string Telefone { get; set; } = "";
    [StringLength(500)]
    public string? FotoPerfil { get; set; }
    public ICollection<Atendimento> Atendimentos { get; set; } = new List<Atendimento>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
