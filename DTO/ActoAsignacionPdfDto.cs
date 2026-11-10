namespace ActosAdultosAPI.DTO
{
    public class ActoAsignacionPdfDto
    {
        public string Numero { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime Inicio { get; set; }

        public DateTime Fin { get; set; }
    }
}
