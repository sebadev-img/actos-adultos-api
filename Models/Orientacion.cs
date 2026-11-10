namespace ActosAdultosAPI.Models
{
    public class Orientacion: Entity
    {
        public string Descripcion { get; set; }

        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
    }
}
