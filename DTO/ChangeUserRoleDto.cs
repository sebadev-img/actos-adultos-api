using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class ChangeUserRoleDto
    {
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Role { get; set; } = string.Empty;
    }
}
