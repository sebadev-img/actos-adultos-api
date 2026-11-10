using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class AsignarUsuarioDto
    {
        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        public string UsuarioId { get; set; } = string.Empty;
    }
}
