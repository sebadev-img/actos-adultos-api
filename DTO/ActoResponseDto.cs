namespace ActosAdultosAPI.DTO
{
    public class ActoResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }

        public bool EsActivo { get; set; }
        public bool PermiteCargaCargos { get; set; }
        public bool PuedeCargarCargos { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        public string? CreadoPor { get; set; }
        public string? ActualizadoPor { get; set; }
    }
}
