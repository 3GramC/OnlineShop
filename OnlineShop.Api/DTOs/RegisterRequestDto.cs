using System.ComponentModel.DataAnnotations;
namespace OnlineShop.Api.DTOs
{
    public class RegisterRequestDto
    {
        [Required (ErrorMessage = "Full name is required.")]
        [MinLength (6 , ErrorMessage = "Full name must be at least 6 characters.")]
        [MaxLength(60, ErrorMessage = "Full name can be a maximum of 60 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(25, ErrorMessage = "Password can be a maximum of 25 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage ="Confirm password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords must match.")]
        public string ConfirmPassword {  get; set; } = string.Empty;

    }
}
