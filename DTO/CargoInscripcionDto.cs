using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class CargoInscripcionDto
    {
        public int Id { get; set; }

        public int ActoId { get; set; }
        public string ActoNumero { get; set; } = string.Empty;
        public string ActoDescripcion { get; set; } = string.Empty;

        public int InstitucionId { get; set; }
        public string InstitucionNombre { get; set; } = string.Empty;

        public int OrientacionId { get; set; }
        public string OrientacionDescripcion { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int Codigo { get; set; }
        public int NumeroLlamado { get; set; }

        public Turno Turno { get; set; }
        public Revista Revista { get; set; }

        public int Curso { get; set; }
        public string Division { get; set; } = string.Empty;

        public string PlazaSUNA { get; set; } = string.Empty;
        public string CargaHoraria { get; set; } = string.Empty;
        public string CodigoJunta { get; set; } = string.Empty;
    }
}
