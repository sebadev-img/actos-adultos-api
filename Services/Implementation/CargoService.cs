using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class CargoService: ICargoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public CargoService(
            ApplicationDbContext context,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        private string? GetUsername()
        {
            return _contextAccessor.HttpContext?
                .User.Identity?.Name;
        }

        // AGREGAR CARGO
        public async Task<Cargo> AddAsync(Cargo cargo)
        {
            var now = DateHelper.ArgentinaNow();
            var username = GetUsername();

            cargo.Id = 0;
            cargo.FechaCreacion = now;
            cargo.CreadoPor = username;
            cargo.Eliminado = false;

            foreach (var horario in cargo.Horarios)
            {
                horario.Id = 0;
                horario.FechaCreacion = now;
                horario.CreadoPor = username;
                horario.Eliminado = false;
            }

            _context.Set<Cargo>().Add(cargo);
            await _context.SaveChangesAsync();

            return cargo;
        }

        // EDITAR CARGO Y SUS HORARIOS
        public async Task<bool> UpdateAsync(
            Cargo cargo,
            int institucionId)
        {
            var existing = await _context.Set<Cargo>()
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c =>
                    c.Id == cargo.Id &&
                    c.InstitucionId == institucionId &&
                    !c.Eliminado);

            if (existing == null)
                return false;

            var now = DateHelper.ArgentinaNow();
            var username = GetUsername();

            existing.ActoId = cargo.ActoId;
            existing.OrientacionId = cargo.OrientacionId;
            existing.Turno = cargo.Turno;
            existing.Descripcion = cargo.Descripcion;
            existing.Codigo = cargo.Codigo;
            existing.NumeroLLamado = cargo.NumeroLLamado;
            existing.Revista = cargo.Revista;
            existing.Curso = cargo.Curso;
            existing.Division = cargo.Division;
            existing.PlazaSUNA = cargo.PlazaSUNA;
            existing.CargaHoraria = cargo.CargaHoraria;
            existing.CodigoJunta = cargo.CodigoJunta;

            existing.FechaActualizacion = now;
            existing.ActualizadoPor = username;

            // Soft Delete de los horarios anteriores
            foreach (var horario in existing.Horarios
                         .Where(h => !h.Eliminado))
            {
                horario.Eliminado = true;
                horario.FechaEliminacion = now;
                horario.EliminadoPor = username;
            }

            // Crear los nuevos horarios
            foreach (var horario in cargo.Horarios)
            {
                existing.Horarios.Add(new CargoHorario
                {
                    DiaSemana = horario.DiaSemana,
                    HoraInicio = horario.HoraInicio,
                    HoraFin = horario.HoraFin,

                    FechaCreacion = now,
                    CreadoPor = username,
                    Eliminado = false
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // OBTENER CARGO POR ID Y POR INSTITUCIÓN
        public async Task<Cargo?> GetByIdAsync(
            int id,
            int institucionId)
        {
            return await _context.Set<Cargo>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Acto)
                .Include(c => c.Institucion)
                .Include(c => c.Orientacion)
                .Include(c => c.Horarios.Where(h => !h.Eliminado))
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.InstitucionId == institucionId &&
                    !c.Eliminado);
        }

        // LISTADO PAGINADO POR INSTITUCIÓN
        public async Task<PagedResponse<Cargo>> GetPagedAsync(
            CargoQueryRequest request,
            int institucionId)
        {
            var query = _context.Set<Cargo>()
                .AsNoTracking()
                .Where(c =>
                    !c.Eliminado &&
                    c.InstitucionId == institucionId);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(c =>
                    c.Descripcion.Contains(search) ||
                    c.CodigoJunta.Contains(search) ||
                    c.PlazaSUNA.Contains(search));
            }

            if (request.ActoId.HasValue)
            {
                query = query.Where(c =>
                    c.ActoId == request.ActoId.Value);
            }

            if (request.OrientacionId.HasValue)
            {
                query = query.Where(c =>
                    c.OrientacionId == request.OrientacionId.Value);
            }

            if (request.Turno.HasValue)
            {
                query = query.Where(c =>
                    c.Turno == request.Turno.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.FechaCreacion)
                .ThenByDescending(c => c.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsSplitQuery()
                .Include(c => c.Acto)
                .Include(c => c.Institucion)
                .Include(c => c.Orientacion)
                .Include(c => c.Horarios.Where(h => !h.Eliminado))
                .ToListAsync();

            return new PagedResponse<Cargo>
            {
                Items = items,
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResponse<Cargo>> GetPagedAdminAsync(
            CargoQueryRequest request)
        {
            var page = Math.Max(1, request.Page);

            var pageSize = Math.Clamp(
                request.PageSize,
                1,
                100);

            var query = _context.Set<Cargo>()
                .AsNoTracking()
                .Where(c =>
                    !c.Eliminado &&
                    !c.Institucion.Eliminado &&
                    !c.Acto.Eliminado)
                .Include(c => c.Acto)
                .Include(c => c.Institucion)
                .Include(c => c.Orientacion)
                .Include(c => c.Horarios)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(c =>
                    c.Descripcion.Contains(search) ||
                    c.Institucion.Descripcion.Contains(search));
            }

            if (request.ActoId.HasValue)
            {
                query = query.Where(c =>
                    c.ActoId == request.ActoId.Value);
            }

            if (request.InstitucionId.HasValue)
            {
                query = query.Where(c =>
                    c.InstitucionId ==
                    request.InstitucionId.Value);
            }

            if (request.OrientacionId.HasValue)
            {
                query = query.Where(c =>
                    c.OrientacionId ==
                    request.OrientacionId.Value);
            }

            if (request.Turno.HasValue)
            {
                query = query.Where(c =>
                    c.Turno == request.Turno.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Institucion.Descripcion)
                .ThenBy(c => c.Descripcion)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Cargo>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
