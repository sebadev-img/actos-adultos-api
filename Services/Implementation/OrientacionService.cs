using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class OrientacionService: IOrientacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public OrientacionService(
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

        // AGREGAR ORIENTACIÓN
        public async Task AddAsync(Orientacion orientacion)
        {
            orientacion.Id = 0;
            orientacion.FechaCreacion = DateHelper.ArgentinaNow();
            orientacion.CreadoPor = GetUsername();

            orientacion.Eliminado = false;
            orientacion.FechaEliminacion = null;
            orientacion.EliminadoPor = null;

            await _context.Set<Orientacion>().AddAsync(orientacion);
            await _context.SaveChangesAsync();
        }

        // EDITAR ORIENTACIÓN
        public async Task UpdateAsync(Orientacion orientacion)
        {
            var existing = await _context.Set<Orientacion>()
                .FirstOrDefaultAsync(o =>
                    o.Id == orientacion.Id && !o.Eliminado);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "La orientación no existe o fue eliminada.");
            }

            existing.Descripcion = orientacion.Descripcion;

            existing.FechaActualizacion = DateHelper.ArgentinaNow();
            existing.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();
        }

        // ELIMINAR ORIENTACIÓN (SOFT DELETE)
        public async Task DeleteAsync(Orientacion orientacion)
        {
            var existing = await _context.Set<Orientacion>()
                .FirstOrDefaultAsync(o =>
                    o.Id == orientacion.Id && !o.Eliminado);

            if (existing == null)
            {
                throw new KeyNotFoundException(
                    "La orientación no existe o ya fue eliminada.");
            }

            existing.Eliminado = true;
            existing.FechaEliminacion = DateHelper.ArgentinaNow();
            existing.EliminadoPor = GetUsername();

            await _context.SaveChangesAsync();
        }

        // OBTENER POR ID
        public async Task<Orientacion?> GetByIdAsync(int id)
        {
            return await _context.Set<Orientacion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(o =>
                    o.Id == id && !o.Eliminado);
        }

        // LISTADO PAGINADO
        public async Task<PagedResponse<Orientacion>> GetPagedAsync(
            OrientacionQueryRequest request)
        {
            var query = _context.Set<Orientacion>()
                .AsNoTracking()
                .Where(o => !o.Eliminado);

            // FILTRO POR DESCRIPCIÓN
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(o =>
                    o.Descripcion.Contains(search));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(o => o.Descripcion)
                .ThenBy(o => o.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Orientacion>
            {
                Items = items,
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
