using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class ActoService: IActoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public ActoService(
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

        // CREAR ACTO
        public async Task AddAsync(Acto acto)
        {
            acto.Id = 0;
            acto.FechaCreacion = DateHelper.ArgentinaNow();
            acto.CreadoPor = GetUsername();

            acto.Eliminado = false;
            acto.FechaEliminacion = null;
            acto.EliminadoPor = null;

            await _context.Set<Acto>().AddAsync(acto);
            await _context.SaveChangesAsync();
        }

        // ACTUALIZAR ACTO
        public async Task UpdateAsync(Acto acto)
        {
            var existingActo = await _context.Set<Acto>()
                .FirstOrDefaultAsync(a =>
                    a.Id == acto.Id && !a.Eliminado);

            if (existingActo == null)
            {
                throw new KeyNotFoundException(
                    "El acto no existe o fue eliminado.");
            }

            // Actualizar únicamente los campos editables
            existingActo.Numero = acto.Numero;
            existingActo.Descripcion = acto.Descripcion;
            existingActo.Inicio = acto.Inicio;
            existingActo.Fin = acto.Fin;
            existingActo.EsActivo = acto.EsActivo;
            existingActo.PermiteCargaCargos = acto.PermiteCargaCargos;

            // Auditoría
            existingActo.FechaActualizacion =
                DateHelper.ArgentinaNow();

            existingActo.ActualizadoPor = GetUsername();

            await _context.SaveChangesAsync();
        }

        // ELIMINAR ACTO (SOFT DELETE)
        public async Task DeleteAsync(Acto acto)
        {
            var existingActo = await _context.Set<Acto>()
                .FirstOrDefaultAsync(a =>
                    a.Id == acto.Id && !a.Eliminado);

            if (existingActo == null)
            {
                throw new KeyNotFoundException(
                    "El acto no existe o ya fue eliminado.");
            }

            existingActo.Eliminado = true;
            existingActo.FechaEliminacion =
                DateHelper.ArgentinaNow();
            existingActo.EliminadoPor = GetUsername();

            await _context.SaveChangesAsync();
        }

        // OBTENER ACTO POR ID
        public async Task<Acto?> GetByIdAsync(
            int? id,
            List<string>? includes = null)
        {
            if (id == null)
                return null;

            IQueryable<Acto> query = _context.Set<Acto>()
                .AsNoTracking()
                .Where(a => !a.Eliminado);

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(
                a => a.Id == id.Value);
        }

        // OBTENER ACTOS PAGINADOS
        public async Task<PagedResponse<Acto>> GetPagedAsync(
            ActoQueryRequest request)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(
                request.PageSize, 1, 100);

            var query = _context.Set<Acto>()
                .AsNoTracking()
                .Where(a => !a.Eliminado);

            // FILTRO POR NÚMERO O DESCRIPCIÓN
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(a =>
                    a.Numero.Contains(search) ||
                    a.Descripcion.Contains(search));
            }

            // FILTRO FECHA DESDE
            if (request.FechaDesde.HasValue)
            {
                var fechaDesde = request.FechaDesde.Value.Date;

                query = query.Where(a =>
                    a.Inicio >= fechaDesde);
            }

            // FILTRO FECHA HASTA (INCLUYE TODO EL DÍA)
            if (request.FechaHasta.HasValue)
            {
                var fechaHastaExclusiva =
                    request.FechaHasta.Value.Date.AddDays(1);

                query = query.Where(a =>
                    a.Inicio < fechaHastaExclusiva);
            }

            // FILTRO POR HABILITADO PARA  CARGAR  CARGOS

            if (request.PermiteCargaCargos.HasValue)
            {
                query = query.Where(a =>
                    a.PermiteCargaCargos ==
                    request.PermiteCargaCargos.Value);
            }

            // FILTRO POR DISPONIBILIDAD PARA CARGAR CARGOS
            if (request.PuedeCargarCargos.HasValue)
            {
                var ahora = DateTime.Now;

                if (request.PuedeCargarCargos.Value)
                {
                    query = query.Where(a =>
                        a.PermiteCargaCargos &&
                        ahora < a.Inicio.AddHours(-1));
                }
                else
                {
                    query = query.Where(a =>
                        !a.PermiteCargaCargos ||
                        ahora >= a.Inicio.AddHours(-1));
                }
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.FechaCreacion)
                .ThenByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Acto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ActoDisponibleDto>> GetDisponiblesAsync(
            DateTime ahora,
            int? postulanteId = null)
        {
            var tienePostulante = postulanteId.HasValue;
            var postulanteIdValue = postulanteId ?? 0;

            return await _context.Set<Acto>()
                .AsNoTracking()
                .Where(a =>
                    !a.Eliminado &&
                    a.EsActivo &&
                    a.Inicio <= ahora &&
                    a.Fin >= ahora)
                .OrderBy(a => a.Fin)
                .ThenBy(a => a.Id)
                .Select(a => new ActoDisponibleDto
                {
                    Id = a.Id,
                    Numero = a.Numero,
                    Descripcion = a.Descripcion,
                    Inicio = a.Inicio,
                    Fin = a.Fin,

                    Cargos = a.Cargos
                        .Where(c =>
                            !c.Eliminado &&
                            !c.Institucion.Eliminado &&
                            !c.Orientacion.Eliminado)
                        .OrderBy(c => c.Descripcion)
                        .ThenBy(c => c.Id)
                        .Select(c => new CargoDisponibleDto
                        {
                            Id = c.Id,

                            InstitucionId = c.InstitucionId,
                            InstitucionNombre =
                                c.Institucion.Descripcion,

                            CiudadId = c.Institucion.CiudadId,
                            CiudadNombre = c.Institucion.Ciudad.Nombre,

                            OrientacionId = c.OrientacionId,
                            OrientacionDescripcion =
                                c.Orientacion.Descripcion,

                            Descripcion = c.Descripcion,
                            Codigo = c.Codigo,
                            NumeroLLamado = c.NumeroLLamado,
                            Turno = c.Turno,
                            Revista = c.Revista,
                            Curso = c.Curso,
                            Division = c.Division,
                            PlazaSUNA = c.PlazaSUNA,
                            CargaHoraria = c.CargaHoraria,
                            CodigoJunta = c.CodigoJunta,

                            Horarios = c.Horarios
                                .Where(h => !h.Eliminado)
                                .OrderBy(h => h.DiaSemana)
                                .ThenBy(h => h.HoraInicio)
                                .Select(h => new CargoHorarioDto
                                {
                                    DiaSemana = h.DiaSemana,
                                    HoraInicio = h.HoraInicio,
                                    HoraFin = h.HoraFin
                                })
                                .ToList(),

                            EstaInscripto = tienePostulante && _context.Set<Inscripcion>()
                            .Any(i =>
                                !i.Eliminado &&
                                i.PostulanteId == postulanteIdValue &&
                                i.CargoId == c.Id),
                        })
                        .ToList()
                })
                .AsSplitQuery()
                .ToListAsync();
        }

    }
}
