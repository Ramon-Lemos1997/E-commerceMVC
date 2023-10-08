using Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Models
{
    public class UpdatePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string CurrPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessage = "O senha deve conter no mínimo 10 caracteres.")]
        [Display(Name = "Nova senha")]
        [PasswordRequirements(ErrorMessage = "A senha deve conter pelo menos 1 número e 1 caractere especial.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }
    }
}
