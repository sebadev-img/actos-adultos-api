using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class PostulanteAddOrUpdateDto
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [Range(1, long.MaxValue,
        ErrorMessage = "El DNI debe ser válido.")]
        public long? Dni { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(150)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(150)]
        public string Apellidos { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Altura { get; set; }

        [StringLength(50)]
        public string? Depto { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una ciudad válida.")]
        public int CiudadId { get; set; }
    }
}
