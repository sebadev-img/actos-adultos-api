using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.DTO
{
    public class InscriptoAsignacionPdfDto
    {
        public long Dni { get; set; }

        public string Apellidos { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;

        public Caracter? Caracter { get; set; }

        public decimal? PuntajeJunta { get; set; }

        public bool EsAsignado { get; set; }
    }
}
