using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RockServers.DTO.Accounts
{
    public class RegisterDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "Please enter a name with atleast 2 Characters.")]
        [MaxLength(255, ErrorMessage = "Please enter a name length with less than 255 Characters.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MinLength(2, ErrorMessage = "Please enter a name with atleast 2 Characters.")]
        [MaxLength(255, ErrorMessage = "Please enter a name length with less than 255 Characters.")]
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "Please enter a name with atleast 2 Characters.")]
        [MaxLength(255, ErrorMessage = "Please enter a name length with less than 255 Characters.")]
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}