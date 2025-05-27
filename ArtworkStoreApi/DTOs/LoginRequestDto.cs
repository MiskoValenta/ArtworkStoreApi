using System.ComponentModel.DataAnnotations;

namespace ArtworkStoreApi.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    
        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
