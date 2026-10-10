using System.ComponentModel.DataAnnotations;

namespace VetCare.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(120, MinimumLength = 3)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = "";

    [Required, StringLength(14, MinimumLength = 11)]
    [Display(Name = "CPF")]
    public string CPF { get; set; } = "";

    [Required, EmailAddress, StringLength(180)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required, Phone, StringLength(30)]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = "";

    [StringLength(180)]
    [Display(Name = "Endereço (opcional)")]
    public string? Endereco { get; set; }

    [Required, MinLength(8), StringLength(100)]
    [DataType(DataType.Password)]
    [Display(Name = "Senha (mínimo 8 caracteres)")]
    public string Senha { get; set; } = "";

    [Required, Compare(nameof(Senha))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirme a senha")]
    public string ConfirmarSenha { get; set; } = "";
}
