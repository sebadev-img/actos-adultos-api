namespace ActosAdultosAPI.DTO
{
    public class InstitucionResponseDto
    {
        public int Id { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public int CiudadId { get; set; }
        public string CiudadNombre { get; set; } = string.Empty;

        public string? UsuarioId { get; set; }
        public string? UsuarioEmail { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        public string? CreadoPor { get; set; }
        public string? ActualizadoPor { get; set; }
    }
}
