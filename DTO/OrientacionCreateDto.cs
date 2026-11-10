using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class OrientacionCreateDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage =
        "La descripción no puede superar los 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
