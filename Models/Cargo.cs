namespace ActosAdultosAPI.Models
{
    public class Cargo: Entity
    {
        public int ActoId { get; set; }
        public Acto Acto { get; set; }

        public int InstitucionId { get; set; }
        public Institucion Institucion { get; set; }

        public int OrientacionId { get; set; }
        public Orientacion Orientacion { get; set; }

        public ICollection<CargoHorario> Horarios { get; set; }
        = new List<CargoHorario>();

        public Turno Turno { get; set; }
        public string Descripcion { get; set; }
        public int Codigo { get; set; }
        public int NumeroLLamado { get; set; }
        public Revista Revista { get; set; }
        public int Curso { get; set; }
        public string Division { get; set; }
        public string PlazaSUNA { get; set; }
        public string CargaHoraria { get; set; }
        public string CodigoJunta { get; set; }

        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();


    }

    public enum Turno
    {
        MAÑANA = 0,
        TARDE = 1,
        VESPERTINO = 2,
    }

    public enum Revista
    {
        TITULAR = 0,
        INTERINO = 1,
        SUPLENTE = 2,
    }
}
