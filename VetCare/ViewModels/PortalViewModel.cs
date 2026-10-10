using VetCare.Models;

namespace VetCare.ViewModels;

public class PortalViewModel
{
    public Tutor Tutor { get; set; } = new();
    public List<Animal> Animais { get; set; } = [];
    public List<Agendamento> Agendamentos { get; set; } = [];
    public List<Atendimento> Atendimentos { get; set; } = [];
    public List<SolicitacaoVinculoAnimal> Solicitacoes { get; set; } = [];
}
