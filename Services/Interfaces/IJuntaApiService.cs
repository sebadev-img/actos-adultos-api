using ActosAdultosAPI.DTO;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IJuntaApiService
    {
        Task<List<CodigoJuntaDto>> GetCodigosJuntaAsync(
            CancellationToken cancellationToken = default);
        Task<JuntaConsultaDto?> ConsultarPuntajeAsync(
            long dni,
            string codigoJunta,
            string ciudad,
            CancellationToken cancellationToken = default);
    }
}
