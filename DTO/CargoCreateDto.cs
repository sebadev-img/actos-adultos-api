using ActosAdultosAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class CargoCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ActoId { get; set; }

        [Range(1, int.MaxValue)]
        public int OrientacionId { get; set; }

        public Turno Turno { get; set; }

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public int Codigo { get; set; }

        public int NumeroLLamado { get; set; }

        public Revista Revista { get; set; }

        public int Curso { get; set; }

        [Required]
        public string Division { get; set; } = string.Empty;

        [Required]
        public string PlazaSUNA { get; set; } = string.Empty;

        [Required]
        public string CargaHoraria { get; set; } = string.Empty;

        [Required]
        public string CodigoJunta { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage =
            "Debe agregar al menos un horario.")]
        public List<CargoHorarioDto> Horarios { get; set; } = new();
    }
}
