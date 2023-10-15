using System.ComponentModel.DataAnnotations;
using System.Data;


namespace Contracts.Models
{
    public class InfoUserModel
    {
        public string Id { get; set; }

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
        [MaxLength(30)]
        [Display(Name = "Gênero")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "O campo Estado é obrigatório.")]
        [MaxLength(30)]
        [Display(Name = "Estado")]
        public string State { get; set; }

        [Required(ErrorMessage = "O campo País é obrigatório.")]
        [MaxLength(50)]
        [Display(Name = "País")]
        public string Nation { get; set; }

        [Required(ErrorMessage = "O campo Rua é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Rua")]
        public string Street { get; set; }

        [Required(ErrorMessage = "O campo Bairro é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [MinLength(8, ErrorMessage = "O CEP deve conter exatamente 8 dígitos.")]
        [MaxLength(8, ErrorMessage = "O CEP deve conter exatamente 8 dígitos.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "O campo Número de telefone deve conter apenas números.")]
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

        [Display(Name ="Privilégio")]
        public string UserRole { get; set; }

        public List<string> Roles { get; set; }

    }
}
