namespace ActosAdultosAPI.Models
{
    public class Institucion: Entity
    {
        public string Descripcion { get; set; }
        public int CiudadId { get; set; }
        public Ciudad Ciudad { get; set; }
        public string? UsuarioId { get; set; }
        public AppUser? Usuario { get; set; }

        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
    }
}
