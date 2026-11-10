namespace ActosAdultosAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;
        public AppUser Usuario { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaRevocacion { get; set; }
    }
}
