using System.ComponentModel.DataAnnotations;

namespace VetCare.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)]
    public string Senha { get; set; } = "";
}
