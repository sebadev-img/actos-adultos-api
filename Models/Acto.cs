namespace ActosAdultosAPI.Models
{
    public class Acto: Entity
    {
        public string Numero { get; set; }
        public string Descripcion { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }

        // Control de publicación
        public bool EsActivo { get; set; } = false;
        // Habilitar carga de cargos por instituciones
        public bool PermiteCargaCargos { get; set; } = false;

        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
    }
}
