using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IPostulanteService
    {
        Task<Postulante> AddAsync(Postulante postulante);

        Task<bool> UpdateAsync(Postulante postulante);

        Task<Postulante?> GetByIdAsync(int id);

        Task<Postulante?> GetMiPostulanteAsync();
        
        Task<Postulante?> GetByDniAsync(long dni);

        Task<PagedResponse<Postulante>> GetPagedAsync(PostulanteQueryRequest request);
    }
}
