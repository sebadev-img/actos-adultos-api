using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IInscripcionService
    {
        Task<Cargo?> GetCargoParaInscripcionAsync(int cargoId);

        Task<bool> ExisteInscripcionAsync(
            int cargoId,
            int postulanteId);

        Task<Inscripcion> AddAsync(Inscripcion inscripcion);

        Task<PagedResponse<Inscripcion>> GetByPostulanteIdAsync(
            int postulanteId,
            int page,
            int pageSize);

        Task<PagedResponse<Inscripcion>> GetByCargoIdAsync(
            int cargoId,
            int page,
            int pageSize);

        Task<Inscripcion?> GetByIdParaAsignacionAsync(
            int inscripcionId);

        Task<bool> ExisteAsignadoEnCargoAsync(
            int cargoId,
            int? excluirInscripcionId = null);

        Task<Inscripcion?> AsignarAsync(
            int inscripcionId,
            int institucionId);

        Task<Inscripcion?> QuitarAsignacionAsync(
            int inscripcionId,
            int institucionId);

        Task<CargoAsignacionPdfDto?> GetDatosAsignacionPdfAsync(
            int cargoId);
    }
}
