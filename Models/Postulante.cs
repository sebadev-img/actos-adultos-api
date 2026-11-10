namespace ActosAdultosAPI.Models
{
    public class Postulante: Entity
    {
        public long Dni { get; set; }

        public string Nombres { get; set; }

        public string Apellidos { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        public string? Altura { get; set; }

        public string? Depto { get; set; }
        public int CiudadId { get; set; }
        public Ciudad Ciudad { get; set; } = null!;

        // Relación obligatoria con AppUser
        public string UsuarioId { get; set; } = null!;
        public AppUser Usuario { get; set; } = null!;

        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    }
}
