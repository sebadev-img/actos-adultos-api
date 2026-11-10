using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface ICargoService
    {
        Task<Cargo> AddAsync(Cargo cargo);

        Task<bool> UpdateAsync(
            Cargo cargo,
            int institucionId);

        Task<Cargo?> GetByIdAsync(
            int id,
            int institucionId);

        Task<PagedResponse<Cargo>> GetPagedAsync(
            CargoQueryRequest request,
            int institucionId);

        Task<PagedResponse<Cargo>> GetPagedAdminAsync(
            CargoQueryRequest request);
    }
}
