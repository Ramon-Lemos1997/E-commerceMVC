using System.ComponentModel.DataAnnotations;

namespace Domain.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PasswordRequirementsAttribute : ValidationAttribute
    {
        private const string SpecialCharacters = "!@#$%^&*()_+";

        public PasswordRequirementsAttribute() : base("A senha deve conter pelo menos 1 número e 1 caractere especial.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value is string password)
            {
                // Verifica se há pelo menos 1 número
                bool hasNumber = password.Any(char.IsDigit);

                // Verifica se há pelo menos 1 caractere especial
                bool hasSpecialCharacter = password.Intersect(SpecialCharacters).Any();

                return hasNumber && hasSpecialCharacter;
            }

            return false;
        }
    }
}

