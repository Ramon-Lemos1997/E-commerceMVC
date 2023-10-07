using Contracts.Enums;
using System.ComponentModel.DataAnnotations;


namespace Contracts.Models
{
    public class DataUserModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [MaxLength(255)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "O campo Sobrenome é obrigatório.")]
        [MaxLength(255)]
        [Display(Name = "Sobrenome")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "O campo Gênero é obrigatório.")]
        [Display(Name = "Gênero")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "O campo Rua é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Rua")]
        public string Street { get; set; }

        [Required(ErrorMessage = "O campo Bairro é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [StringLength(8, ErrorMessage = "O CEP deve conter exatamente 8 dígitos.")]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; }     
     
        [Required(ErrorMessage = "O campo Data de nascimento é obrigatório.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de nascimento")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "O campo Número de telefone é obrigatório.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "O campo Número de telefone deve conter apenas números.")]
        [MaxLength(20)]
        [Display(Name = "Número de telefone")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "O campo Cidade é obrigatório.")] 
        [MaxLength(20)]
        [Display(Name = "Cidade")]
        public string City { get; set; }

        [Required(ErrorMessage = "O campo Número é obrigatório.")]
        [MaxLength(10)]
        [Display(Name = "Número")]
        public string HouseNumber { get; set; }

    }
}
