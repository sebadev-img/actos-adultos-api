namespace ActosAdultosAPI.DTO
{
    public class PostulanteInscripcionDto
    {
        public int Id { get; set; }

        public long Dni { get; set; }

        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Telefono { get; set; }

        public string? Direccion { get; set; }
        public string? Altura { get; set; }
        public string? Depto { get; set; }

        public int CiudadId { get; set; }
        public string CiudadNombre { get; set; } = string.Empty;
    }
}
