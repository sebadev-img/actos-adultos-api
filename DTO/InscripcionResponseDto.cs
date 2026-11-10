using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class InscripcionResponseDto
    {
        public int Id { get; set; }
        public int CargoId { get; set; }
        public int PostulanteId { get; set; }

        public EstadoInscripcion EstadoInscripcion { get; set; }

        public decimal? PuntajeJunta { get; set; }
        public Caracter? Caracter { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
