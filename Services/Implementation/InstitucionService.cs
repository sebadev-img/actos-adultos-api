using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ActosAdultosAPI.Services.Implementation
{
    public class InstitucionService : IInstitucionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public InstitucionService(
            ApplicationDbContext context,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        private string? GetUsername()
        {
            return _contextAccessor.HttpContext?.User.Identity?.Name;
        }

        private string? GetUserId()
        {
            return _contextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // CREATE
        public async Task<Institucion> AddAsync(Institucion institucion)
        {
            institucion.FechaCreacion = DateHelper.ArgentinaNow();
            institucion.CreadoPor = GetUsername();

            institucion.UsuarioId = null;
            institucion.Eliminado = false;

            _context.Set<Institucion>().Add(institucion);
            await _context.SaveChangesAsync();

            return institucion;
        }

        // UPDATE
        public async Task<bool> UpdateAsync(Institucion institucion)
        {
            var existing = await _context.Set<Institucion>()
                .Where(i => !i.Eliminado)
                .FirstOrDefaultAsync(i => i.Id == institucion.Id);

            if (existing == null)
                return false;

            existing.Descripcion = institucion.Descripcion;
            existing.CiudadId = institucion.CiudadId;

            existing.FechaActualizacion = DateHelper.ArgentinaNow();
            existing.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();

            return true;
        }

        // DELETE (SOFT DELETE)
        public async Task<bool> DeleteAsync(int id)
        {
            var institucion = await _context.Set<Institucion>()
                .Where(i => !i.Eliminado)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (institucion == null)
                return false;

            institucion.Eliminado = true;
            institucion.FechaEliminacion = DateHelper.ArgentinaNow();
            institucion.EliminadoPor = GetUsername();

            institucion.UsuarioId = null;

            await _context.SaveChangesAsync();

            return true;
        }

        // GET BY ID
        public async Task<Institucion?> GetByIdAsync(int id)
        {
            return await _context.Set<Institucion>()
                .AsNoTracking()
                .Where(i => !i.Eliminado)
                .Include(i => i.Ciudad)
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // GET PAGED
        public async Task<PagedResponse<Institucion>> GetPagedAsync(
            InstitucionQueryRequest request)
        {
            var query = _context.Set<Institucion>()
                .AsNoTracking()
                .Where(i => !i.Eliminado)
                .Include(i => i.Ciudad)
                .Include(i => i.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(i =>
                    i.Descripcion.Contains(request.Search));
            }

            if (request.CiudadId.HasValue)
            {
                query = query.Where(i =>
                    i.CiudadId == request.CiudadId.Value);
            }

            if (request.TieneUsuario.HasValue)
            {
                query = query.Where(i =>
                    (i.UsuarioId != null) == request.TieneUsuario.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(i => i.Descripcion)
                .ThenBy(i => i.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Institucion>
            {
                Items = items,
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        // ASIGNAR USUARIO
        public async Task AsignarUsuarioAsync(
            int institucionId,
            string usuarioId)
        {
            var institucion = await _context.Set<Institucion>()
                .Where(i => !i.Eliminado)
                .FirstAsync(i => i.Id == institucionId);

            institucion.UsuarioId = usuarioId;
            institucion.FechaActualizacion = DateHelper.ArgentinaNow();
            institucion.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();
        }

        // QUITAR USUARIO
        public async Task<bool> QuitarUsuarioAsync(int institucionId)
        {
            var institucion = await _context.Set<Institucion>()
                .Where(i => !i.Eliminado)
                .FirstOrDefaultAsync(i => i.Id == institucionId);

            if (institucion == null)
                return false;

            institucion.UsuarioId = null;
            institucion.FechaActualizacion = DateHelper.ArgentinaNow();
            institucion.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();

            return true;
        }

        // OBTENER MI INSTITUCIÓN
        public async Task<Institucion?> GetMiInstitucionAsync()
        {
            var userId = GetUserId();

            return await _context.Set<Institucion>()
                .AsNoTracking()
                .Where(i => !i.Eliminado)
                .Include(i => i.Ciudad)
                .FirstOrDefaultAsync(i => i.UsuarioId == userId);
        }

        public async Task<bool> UsuarioAsignadoAOtraInstitucionAsync(
            string usuarioId,
            int institucionId)
        {
            return await _context.Set<Institucion>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(i =>
                    i.UsuarioId == usuarioId &&
                    i.Id != institucionId);
        }

        // TODO EL LISTADO COMPLETO
        public async Task<List<Institucion>> GetAllAsync()
        {
            return await _context.Set<Institucion>()
                .AsNoTracking()
                .Where(i => !i.Eliminado)
                .OrderBy(i => i.Descripcion)
                .ToListAsync();
        }
    }
}
