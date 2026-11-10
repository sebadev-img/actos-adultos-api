using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class ResendConfirmationEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
