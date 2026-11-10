namespace ActosAdultosAPI.DTO
{
    public class OrientacionResponseDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
        public string? CreadoPor { get; set; }

        public DateTime? FechaActualizacion { get; set; }
        public string? ActualizadoPor { get; set; }
    }
}
