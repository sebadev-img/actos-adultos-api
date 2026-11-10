using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IActoService
    {
        Task AddAsync(Acto acto);
        Task UpdateAsync(Acto acto);
        Task DeleteAsync(Acto acto);
        Task<Acto?> GetByIdAsync(int? id, List<string>? includes = null);        
        Task<PagedResponse<Acto>> GetPagedAsync(ActoQueryRequest request);
        Task<List<ActoDisponibleDto>> GetDisponiblesAsync(DateTime ahora, int? postulanteId = null);
    }
}
