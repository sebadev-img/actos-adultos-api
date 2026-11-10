using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IInstitucionService
    {
        // CRUD
        Task<Institucion> AddAsync(Institucion institucion);

        Task<bool> UpdateAsync(Institucion institucion);

        Task<bool> DeleteAsync(int id);

        // Consultas
        Task<Institucion?> GetByIdAsync(int id);

        Task<PagedResponse<Institucion>> GetPagedAsync(
            InstitucionQueryRequest request);

        // Relación con usuarios
        Task AsignarUsuarioAsync(
            int institucionId,
            string usuarioId);

        Task<bool> QuitarUsuarioAsync(int institucionId);

        // Institución del usuario autenticado
        Task<Institucion?> GetMiInstitucionAsync();

        Task<bool> UsuarioAsignadoAOtraInstitucionAsync(string usuarioId, int institucionId);

        Task<List<Institucion>> GetAllAsync();
    }
}
