using System.ComponentModel.DataAnnotations;

namespace VetCare.Models;

public class Servico
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Nome { get; set; } = "";
    [StringLength(500)]
    public string? Descricao { get; set; }
    [Range(0, 999999)]
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
