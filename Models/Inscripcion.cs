namespace ActosAdultosAPI.Models
{
    public class Inscripcion : Entity
    {
        public int CargoId { get; set; }
        public Cargo Cargo { get; set; }

        public int PostulanteId { get; set; }
        public Postulante Postulante { get; set; }

        public EstadoInscripcion EstadoInscripcion { get; set; }
        public DateTime? FechaAsignacion { get; set; }

        public decimal? PuntajeJunta { get; set; }
        public Caracter? Caracter { get; set; }

    }

    public enum EstadoInscripcion
    {
        INSCRIPCION = 0,
        ASIGNADO = 1,
    }

    public enum Caracter
    {
        Docente = 0,
        Habilitante = 1,
        Supletorio = 2,
        Exclusión = 3,
    }
}
