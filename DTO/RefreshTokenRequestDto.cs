using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
