using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class ActoCreateDto
    {
        [Required(ErrorMessage = "El número es obligatorio.")]
        [StringLength(100, MinimumLength = 1)]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, MinimumLength = 1)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime? Inicio { get; set; }

        [Required]
        public DateTime? Fin { get; set; }

        public bool EsActivo { get; set; } = false;
        public bool PermiteCargaCargos { get; set; } = false;
    }
}
