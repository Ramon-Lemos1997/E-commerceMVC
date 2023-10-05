using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(255)]
        public string FirstName { get; set; }

        [MaxLength(255)]
        public string Surname { get; set; }
    
        [MaxLength(50)]
        public string Gender { get; set; }

        [MaxLength(255)]
        public string Adress { get; set; }
        public DateTime BirthDate { get; set; }
        public bool ResetPassword { get; set; }
    }

}

