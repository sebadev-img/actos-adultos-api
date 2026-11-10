namespace ActosAdultosAPI.Models
{
    public class Entity
    {
        public int Id { get; set; }

        // Auditoría de creación
        public DateTime FechaCreacion { get; set; }
        public string? CreadoPor { get; set; }

        // Auditoría de modificación
        public DateTime? FechaActualizacion { get; set; }
        public string? ActualizadoPor { get; set; }

        // Soft Delete
        public bool Eliminado { get; set; } = false;
        public DateTime? FechaEliminacion { get; set; }
        public string? EliminadoPor { get; set; }
    }
}
