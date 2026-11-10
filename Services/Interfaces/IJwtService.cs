using ActosAdultosAPI.Models;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IJwtService
    {
        Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(AppUser user);
    }
}
