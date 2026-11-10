using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IOrientacionService
    {
        Task AddAsync(Orientacion orientacion);

        Task UpdateAsync(Orientacion orientacion);

        Task DeleteAsync(Orientacion orientacion);

        Task<Orientacion?> GetByIdAsync(int id);

        Task<PagedResponse<Orientacion>> GetPagedAsync(
            OrientacionQueryRequest request);
    }
}
