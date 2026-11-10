using ActosAdultosAPI.Data;
using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class InscripcionService: IInscripcionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public InscripcionService(
            ApplicationDbContext context,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        // OBTENER CARGO CON LAS RELACIONES NECESARIAS
        public async Task<Cargo?> GetCargoParaInscripcionAsync(
            int cargoId)
        {
            return await _context.Set<Cargo>()
                .AsNoTracking()
                .Include(c => c.Acto)
                .Include(c => c.Institucion)
                    .ThenInclude(i => i.Ciudad)
                .Include(c => c.Orientacion)
                .FirstOrDefaultAsync(c => c.Id == cargoId);
        }

        // COMPROBAR INSCRIPCIÓN EXISTENTE
        public async Task<bool> ExisteInscripcionAsync(
            int cargoId,
            int postulanteId)
        {
            return await _context.Set<Inscripcion>()
                .AsNoTracking()
                .AnyAsync(i =>
                    i.CargoId == cargoId &&
                    i.PostulanteId == postulanteId &&
                    !i.Eliminado);
        }

        // GUARDAR INSCRIPCIÓN
        public async Task<Inscripcion> AddAsync(
            Inscripcion inscripcion)
        {
            inscripcion.Id = 0;

            inscripcion.FechaCreacion =
                DateHelper.ArgentinaNow();

            inscripcion.CreadoPor =
                _contextAccessor.HttpContext?
                    .User.Identity?.Name;

            inscripcion.Eliminado = false;
            inscripcion.FechaEliminacion = null;
            inscripcion.EliminadoPor = null;

            await _context.Set<Inscripcion>()
                .AddAsync(inscripcion);

            await _context.SaveChangesAsync();

            return inscripcion;
        }

        // OBTENER INSCRIPCIONES DE UN POSTULANTE
        public async Task<PagedResponse<Inscripcion>> GetByPostulanteIdAsync(
            int postulanteId,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Set<Inscripcion>()
                .AsNoTracking()
                .Where(i =>
                    !i.Eliminado &&
                    i.PostulanteId == postulanteId)
                .Include(i => i.Postulante)
                    .ThenInclude(p => p.Ciudad)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Acto)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Institucion)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Orientacion);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.FechaCreacion)
                .ThenByDescending(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Inscripcion>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }


        // OBTENER INSCRIPCIONES DE UN CARGO
        public async Task<PagedResponse<Inscripcion>> GetByCargoIdAsync(
            int cargoId,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Set<Inscripcion>()
                .AsNoTracking()
                .Where(i =>
                    !i.Eliminado &&
                    i.CargoId == cargoId)
                .Include(i => i.Postulante)
                    .ThenInclude(p => p.Ciudad)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Acto)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Institucion)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Orientacion);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.PuntajeJunta)
                .ThenBy(i => i.FechaCreacion)
                .ThenBy(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Inscripcion>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        // OBTENER INSCRIPCIÓN PARA ASIGNACIÓN
        public async Task<Inscripcion?> GetByIdParaAsignacionAsync(
            int inscripcionId)
        {
            return await _context.Set<Inscripcion>()
                .AsNoTracking()
                .Include(i => i.Postulante)
                .Include(i => i.Cargo)
                    .ThenInclude(c => c.Institucion)
                .FirstOrDefaultAsync(i =>
                    i.Id == inscripcionId &&
                    !i.Eliminado);
        }


        // COMPROBAR SI EL CARGO YA TIENE UN ASIGNADO
        public async Task<bool> ExisteAsignadoEnCargoAsync(
            int cargoId,
            int? excluirInscripcionId = null)
        {
            return await _context.Set<Inscripcion>()
                .AsNoTracking()
                .AnyAsync(i =>
                    !i.Eliminado &&
                    i.CargoId == cargoId &&
                    i.EstadoInscripcion ==
                        EstadoInscripcion.ASIGNADO &&
                    (
                        !excluirInscripcionId.HasValue ||
                        i.Id != excluirInscripcionId.Value
                    ));
        }


        // ASIGNAR INSCRIPCIÓN
        public async Task<Inscripcion?> AsignarAsync(
            int inscripcionId,
            int institucionId)
        {
            var inscripcion = await _context.Set<Inscripcion>()
                .Include(i => i.Cargo)
                .FirstOrDefaultAsync(i =>
                    i.Id == inscripcionId &&
                    !i.Eliminado);

            if (inscripcion == null)
            {
                return null;
            }

            // SEGURIDAD:
            // el cargo debe pertenecer a la institución autenticada
            if (
                inscripcion.Cargo == null ||
                inscripcion.Cargo.Eliminado ||
                inscripcion.Cargo.InstitucionId != institucionId
            )
            {
                return null;
            }

            // Ya está asignada
            if (
                inscripcion.EstadoInscripcion ==
                EstadoInscripcion.ASIGNADO
            )
            {
                return inscripcion;
            }

            // Verificar que no exista otro asignado
            var existeOtroAsignado =
                await _context.Set<Inscripcion>()
                    .AsNoTracking()
                    .AnyAsync(i =>
                        !i.Eliminado &&
                        i.CargoId == inscripcion.CargoId &&
                        i.Id != inscripcion.Id &&
                        i.EstadoInscripcion ==
                            EstadoInscripcion.ASIGNADO);

            if (existeOtroAsignado)
            {
                throw new InvalidOperationException(
                    "El cargo ya tiene un postulante asignado.");
            }

            inscripcion.EstadoInscripcion =
                EstadoInscripcion.ASIGNADO;

            inscripcion.FechaAsignacion =
                DateHelper.ArgentinaNow();

            inscripcion.FechaActualizacion =
                DateHelper.ArgentinaNow();

            inscripcion.ActualizadoPor =
                _contextAccessor.HttpContext?
                    .User.Identity?.Name;

            await _context.SaveChangesAsync();

            return inscripcion;
        }

        // QUITAR ASIGNACIÓN
        public async Task<Inscripcion?> QuitarAsignacionAsync(
            int inscripcionId,
            int institucionId)
        {
            var inscripcion = await _context.Set<Inscripcion>()
                .Include(i => i.Cargo)
                .FirstOrDefaultAsync(i =>
                    i.Id == inscripcionId &&
                    !i.Eliminado);

            if (inscripcion == null)
            {
                return null;
            }

            // Validar cargo
            if (
                inscripcion.Cargo == null ||
                inscripcion.Cargo.Eliminado
            )
            {
                return null;
            }

            // La institución solamente puede modificar
            // asignaciones de sus propios cargos.
            if (
                inscripcion.Cargo.InstitucionId !=
                institucionId
            )
            {
                return null;
            }

            // Solo se puede quitar una inscripción
            // que actualmente esté asignada.
            if (
                inscripcion.EstadoInscripcion !=
                EstadoInscripcion.ASIGNADO
            )
            {
                throw new InvalidOperationException(
                    "La inscripción no se encuentra asignada.");
            }

            // Volver al estado de inscripción
            inscripcion.EstadoInscripcion =
                EstadoInscripcion.INSCRIPCION;

            // Ya no tiene fecha de asignación
            inscripcion.FechaAsignacion = null;

            await _context.SaveChangesAsync();

            return inscripcion;
        }

        // OBTENER DATOS PARA PDF DE ASIGNACIÓN
        public async Task<CargoAsignacionPdfDto?> GetDatosAsignacionPdfAsync(
            int cargoId)
        {
            var cargo = await _context.Set<Cargo>()
                .AsNoTracking()
                .Include(c => c.Acto)
                .Include(c => c.Institucion)
                    .ThenInclude(i => i.Ciudad)
                .Include(c => c.Orientacion)
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c =>
                    c.Id == cargoId &&
                    !c.Eliminado);

            if (cargo == null)
            {
                return null;
            }

            var inscriptos = await _context.Set<Inscripcion>()
                .AsNoTracking()
                .Where(i =>
                    !i.Eliminado &&
                    i.CargoId == cargoId &&
                    !i.Postulante.Eliminado)
                .OrderByDescending(i =>
                    i.EstadoInscripcion == EstadoInscripcion.ASIGNADO)
                .ThenByDescending(i => i.PuntajeJunta)
                .ThenBy(i => i.Postulante.Apellidos)
                .ThenBy(i => i.Postulante.Nombres)
                .Select(i => new InscriptoAsignacionPdfDto
                {
                    Dni = i.Postulante.Dni,

                    Apellidos = i.Postulante.Apellidos,

                    Nombres = i.Postulante.Nombres,

                    Caracter = i.Caracter,

                    PuntajeJunta = i.PuntajeJunta,

                    EsAsignado =
                        i.EstadoInscripcion ==
                        EstadoInscripcion.ASIGNADO
                })
                .ToListAsync();

            return new CargoAsignacionPdfDto
            {
                Acto = new ActoAsignacionPdfDto
                {
                    Numero = cargo.Acto?.Numero ?? "",

                    Descripcion =
                        cargo.Acto?.Descripcion ?? "",

                    Inicio =
                        cargo.Acto?.Inicio ?? default,

                    Fin =
                        cargo.Acto?.Fin ?? default
                },

                Cargo = new CargoAsignacionDetallePdfDto
                {
                    Id = cargo.Id,

                    Descripcion = cargo.Descripcion,

                    InstitucionNombre =
                        cargo.Institucion?.Descripcion ?? "",

                    CiudadNombre =
                        cargo.Institucion?.Ciudad?.Nombre ?? "",

                    OrientacionDescripcion =
                        cargo.Orientacion?.Descripcion ?? "",

                    Codigo = cargo.Codigo,

                    NumeroLlamado =
                        cargo.NumeroLLamado,

                    Turno = cargo.Turno,

                    Revista = cargo.Revista,

                    Curso = cargo.Curso,

                    Division = cargo.Division,

                    PlazaSUNA = cargo.PlazaSUNA,

                    CargaHoraria =
                        cargo.CargaHoraria,

                    CodigoJunta =
                        cargo.CodigoJunta,

                    Horarios = cargo.Horarios
                        .Where(h => !h.Eliminado)
                        .OrderBy(h => h.DiaSemana)
                        .ThenBy(h => h.HoraInicio)
                        .Select(h =>
                            new CargoHorarioAsignacionPdfDto
                            {
                                DiaSemana = h.DiaSemana,
                                HoraInicio = h.HoraInicio,
                                HoraFin = h.HoraFin
                            })
                        .ToList()
                },

                Inscriptos = inscriptos
            };
        }
    }
}
