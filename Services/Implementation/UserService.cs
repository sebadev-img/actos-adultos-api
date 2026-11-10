using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class UserService: IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // LISTADO PAGINADO
        public async Task<PagedResponse<UserResponseDto>> GetPagedAsync(
            UserQueryRequest request)
        {
            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(u =>
                    (u.Email != null &&
                     u.Email.Contains(search)) ||
                    (u.UserName != null &&
                     u.UserName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var normalizedRole = request.Role.Trim().ToUpper();

                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        _context.Roles.Any(r =>
                            r.Id == ur.RoleId &&
                            r.NormalizedName == normalizedRole)));
            }

            if (request.SoloDisponiblesInstitucion)
            {
                query = query.Where(u =>
                    !_context.Instituciones.Any(i =>
                        i.UsuarioId == u.Id));
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Email)
                .ThenBy(u => u.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Obtener roles de los usuarios de la página
            var userRoles = await (
                from ur in _context.UserRoles
                join r in _context.Roles
                    on ur.RoleId equals r.Id
                where userIds.Contains(ur.UserId)
                select new
                {
                    ur.UserId,
                    RoleName = r.Name
                }
            ).ToListAsync();

            // Obtener instituciones asociadas
            var instituciones = await _context.Set<Institucion>()
                .AsNoTracking()
                .Where(i =>
                    !i.Eliminado &&
                    i.UsuarioId != null &&
                    userIds.Contains(i.UsuarioId))
                .Select(i => new
                {
                    i.Id,
                    i.Descripcion,
                    i.UsuarioId
                })
                .ToListAsync();

            var items = users.Select(u =>
            {
                var role = userRoles
                    .FirstOrDefault(r => r.UserId == u.Id);

                var institucion = instituciones
                    .FirstOrDefault(i => i.UsuarioId == u.Id);

                return new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    UserName = u.UserName,
                    EmailConfirmed = u.EmailConfirmed,
                    Role = role?.RoleName,
                    InstitucionId = institucion?.Id,
                    InstitucionNombre = institucion?.Descripcion
                };
            }).ToList();

            return new PagedResponse<UserResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        // OBTENER USUARIO POR ID
        public async Task<AppUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        // OBTENER ROLES
        public async Task<IList<string>> GetRolesAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        // VERIFICAR SI EXISTE ROL
        public async Task<bool> RoleExistsAsync(string role)
        {
            return await _roleManager.RoleExistsAsync(role);
        }

        // VERIFICAR INSTITUCIÓN ASOCIADA
        public async Task<bool> HasInstitucionAsync(string userId)
        {
            return await _context.Set<Institucion>()
                .AsNoTracking()
                .AnyAsync(i =>
                    !i.Eliminado &&
                    i.UsuarioId == userId);
        }

        // CAMBIAR ROL
        public async Task<IdentityResult> ChangeRoleAsync(
            AppUser user,
            string newRole)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Count == 1 &&
                string.Equals(
                    currentRoles[0],
                    newRole,
                    StringComparison.OrdinalIgnoreCase))
            {
                return IdentityResult.Success;
            }

            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager
                    .RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                    return removeResult;
            }

            var addResult = await _userManager
                .AddToRoleAsync(user, newRole);

            if (!addResult.Succeeded)
                return addResult;

            // Invalidar cookies de Identity existentes.
            // Los JWT emitidos siguen vigentes hasta vencer.
            var stampResult = await _userManager
                .UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
                return stampResult;

            await transaction.CommitAsync();

            return IdentityResult.Success;
        }
    }
}
