namespace ActosAdultosAPI.DTO
{
    public class ActoDisponibleDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }

        public List<CargoDisponibleDto> Cargos { get; set; } = new();
    }
}
