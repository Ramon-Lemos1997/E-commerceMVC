using System.ComponentModel.DataAnnotations;

namespace Contracts.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [Display(Name = "ID do Usuário")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Token de Redefinição de Senha")]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }
    }
}
