using System.ComponentModel.DataAnnotations;
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

        public DateTime BirthDate { get; set; }
        public bool ResetPassword { get; set; }

        [MaxLength(100)]
        public string Street { get; set; } 

        [MaxLength(100)]
        public string Neighborhood { get; set; } 

        [MaxLength(8)]
        public string ZipCode { get; set; } 
    }

}

