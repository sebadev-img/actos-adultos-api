using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class CargoAsignacionDetallePdfDto
    {
        public int Id { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string InstitucionNombre { get; set; } = string.Empty;

        public string CiudadNombre { get; set; } = string.Empty;

        public string OrientacionDescripcion { get; set; } = string.Empty;

        public int Codigo { get; set; }

        public int NumeroLlamado { get; set; }

        public Turno Turno { get; set; }

        public Revista Revista { get; set; }

        public int Curso { get; set; }

        public string Division { get; set; } = string.Empty;

        public string PlazaSUNA { get; set; } = string.Empty;

        public string CargaHoraria { get; set; } = string.Empty;

        public string CodigoJunta { get; set; } = string.Empty;

        public List<CargoHorarioAsignacionPdfDto> Horarios { get; set; } = new();
    }
}
