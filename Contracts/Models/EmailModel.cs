using System.ComponentModel.DataAnnotations;

namespace Contracts.Models
{
    public class EmailModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um email válido.")]
        public string Email { get; set; }
    }
}

