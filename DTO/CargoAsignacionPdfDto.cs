namespace ActosAdultosAPI.DTO
{
    public class CargoAsignacionPdfDto
    {
        public ActoAsignacionPdfDto Acto { get; set; } = new();

        public CargoAsignacionDetallePdfDto Cargo { get; set; } = new();

        public List<InscriptoAsignacionPdfDto> Inscriptos { get; set; } = new();
    }
}
