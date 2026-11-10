using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace ActosAdultosAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponseDto>> GetPagedAsync(
        UserQueryRequest request);

        Task<AppUser?> GetByIdAsync(string userId);

        Task<IList<string>> GetRolesAsync(AppUser user);

        Task<bool> RoleExistsAsync(string role);

        Task<bool> HasInstitucionAsync(string userId);

        Task<IdentityResult> ChangeRoleAsync(
            AppUser user,
            string newRole);
    }
}
