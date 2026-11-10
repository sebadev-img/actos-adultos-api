using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class InstitucionCreateDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, MinimumLength = 1)]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una ciudad válida.")]
        public int CiudadId { get; set; }
    }
}
