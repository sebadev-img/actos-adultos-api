using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class ResetPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Las contraseñas no coinciden."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
