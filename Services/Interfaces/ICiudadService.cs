using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface ICiudadService
    {
        Task<List<Ciudad>> GetCiudades();
        Task<bool> ExistsByIdAsync(int ciudadId);
    }
}
