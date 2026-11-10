using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ActosAdultosAPI.Services.Implementation
{
    public class PostulanteService : IPostulanteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public PostulanteService(
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

        // AGREGAR POSTULANTE
        public async Task<Postulante> AddAsync(Postulante postulante)
        {
            postulante.FechaCreacion = DateHelper.ArgentinaNow();
            postulante.CreadoPor = GetUsername();
            postulante.Eliminado = false;

            _context.Set<Postulante>().Add(postulante);

            await _context.SaveChangesAsync();

            return postulante;
        }

        // EDITAR POSTULANTE
        public async Task<bool> UpdateAsync(Postulante postulante)
        {
            var existing = await _context.Set<Postulante>()
                .Where(p => !p.Eliminado)
                .FirstOrDefaultAsync(p => p.Id == postulante.Id);

            if (existing == null)
                return false;

            // Datos personales
            existing.Dni = postulante.Dni;
            existing.Nombres = postulante.Nombres;
            existing.Apellidos = postulante.Apellidos;
            existing.Email = postulante.Email;
            existing.Telefono = postulante.Telefono;

            // Domicilio
            existing.Direccion = postulante.Direccion;
            existing.Altura = postulante.Altura;
            existing.Depto = postulante.Depto;
            existing.CiudadId = postulante.CiudadId;

            // Auditoría
            existing.FechaActualizacion = DateHelper.ArgentinaNow();
            existing.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();

            return true;
        }

        // OBTENER POR ID
        public async Task<Postulante?> GetByIdAsync(int id)
        {
            return await _context.Set<Postulante>()
                .AsNoTracking()
                .Where(p => !p.Eliminado)
                .Include(p => p.Ciudad)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // OBTENER MI POSTULANTE
        public async Task<Postulante?> GetMiPostulanteAsync()
        {
            var userId = GetUserId();

            return await _context.Set<Postulante>()
                .AsNoTracking()
                .Where(p => !p.Eliminado)
                .Include(p => p.Ciudad)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);
        }
        

        public async Task<Postulante?> GetByDniAsync(long dni)
        {
            return await _context.Set<Postulante>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Dni == dni);
        }

        // OBTENER POSTULANTES PAGINADOS
        public async Task<PagedResponse<Postulante>> GetPagedAsync(
            PostulanteQueryRequest request)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(
                request.PageSize,
                1,
                100);

            var query = _context.Set<Postulante>()
                .AsNoTracking()
                .Where(p => !p.Eliminado)
                .Include(p => p.Ciudad)
                .AsQueryable();

            // BUSCADOR
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                if (long.TryParse(search, out var dni))
                {
                    query = query.Where(p =>
                        p.Dni == dni ||
                        p.Nombres.Contains(search) ||
                        p.Apellidos.Contains(search) ||
                        (p.Email != null &&
                         p.Email.Contains(search)));
                }
                else
                {
                    query = query.Where(p =>
                        p.Nombres.Contains(search) ||
                        p.Apellidos.Contains(search) ||
                        (p.Email != null &&
                         p.Email.Contains(search)));
                }
            }

            // FILTRO POR CIUDAD
            if (request.CiudadId.HasValue)
            {
                query = query.Where(p =>
                    p.CiudadId == request.CiudadId.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Postulante>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
