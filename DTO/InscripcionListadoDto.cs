using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class InscripcionListadoDto
    {
        public int Id { get; set; }

        public EstadoInscripcion EstadoInscripcion { get; set; }
        public DateTime? FechaAsignacion { get; set; }

        public decimal? PuntajeJunta { get; set; }
        public Caracter? Caracter { get; set; }

        public DateTime FechaCreacion { get; set; }

        public PostulanteInscripcionDto Postulante { get; set; } = null!;
        public CargoInscripcionDto Cargo { get; set; } = null!;
    }
}
