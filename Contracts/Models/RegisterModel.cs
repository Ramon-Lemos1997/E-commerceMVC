using System.ComponentModel.DataAnnotations;

namespace Contracts.Models
{

    public class RegisterModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "O campo Password é obrigatório.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirme a senha")]
        [Compare("Password", ErrorMessage = "As senhas devem ser iguais")]
        public string? ConfirmPassword { get; set; }

        public DateTime CreationDate { get; set; }
    }


}
