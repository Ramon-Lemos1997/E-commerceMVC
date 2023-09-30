using System.ComponentModel.DataAnnotations;

namespace E_commerce.MVC_ASP.NET.Models
{
    public class EmailModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um email válido.")]
        public string Email { get; set; }
    }
}

