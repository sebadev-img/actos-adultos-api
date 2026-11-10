using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class AsignarInscripcionResponseDto
    {
        public int Id { get; set; }

        public int CargoId { get; set; }

        public int PostulanteId { get; set; }

        public EstadoInscripcion EstadoInscripcion { get; set; }

        public DateTime? FechaAsignacion { get; set; }
    }
}
