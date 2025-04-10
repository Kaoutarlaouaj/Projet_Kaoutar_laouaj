using System.ComponentModel.DataAnnotations;

namespace Blazorcrud.Client.Shared
{
    public class Register
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string Role { get; set; } = "employee"; // par défaut
    }

}
