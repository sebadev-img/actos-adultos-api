using ActosAdultosAPI.DTO;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IInscripcionPdfService
    {
        byte[] GenerarPdf(
            CargoAsignacionPdfDto datos);
    }
}
