using ActosAdultosAPI.DTO;
using ActosAdultosAPI.EmailTemplates;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InscripcionController : ControllerBase
    {
        private readonly IInscripcionService _service;
        private readonly IPostulanteService _postulanteService;
        private readonly IJuntaApiService _juntaApiService;
        private readonly IInstitucionService _institucionService;
        private readonly IInscripcionPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InscripcionController> _logger;

        public InscripcionController(
            IInscripcionService service,
            IPostulanteService postulanteService,
            IJuntaApiService juntaApiService,
            IInstitucionService instutcionService,
            IInscripcionPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuracion,
            ILogger<InscripcionController> logger)
        {
            _service = service;
            _postulanteService = postulanteService;
            _juntaApiService = juntaApiService;
            _institucionService = instutcionService;
            _pdfService = pdfService;
            _emailService = emailService;
            _configuration = configuracion;
            _logger = logger;
        }

        // REALIZAR INSCRIPCIÓN
        [HttpPost]
        [Authorize(Roles = "admin,docente")]
        public async Task<IActionResult> Add(
            [FromBody] InscripcionCreateDto request,
            CancellationToken cancellationToken)
        {
            // 1. Obtener postulante del usuario autenticado
            var postulante = await _postulanteService
                .GetMiPostulanteAsync();

            if (postulante == null || postulante.Eliminado)
            {
                return BadRequest(new
                {
                    Message = "Debés completar tu perfil de postulante antes de inscribirte."
                });
            }

            if (postulante.Dni <= 0)
            {
                return BadRequest(new
                {
                    Message = "El DNI del postulante no es válido."
                });
            }

            // 2. Obtener cargo con acto e institución
            var cargo = await _service
                .GetCargoParaInscripcionAsync(request.CargoId);

            if (cargo == null || cargo.Eliminado)
            {
                return NotFound(new
                {
                    Message = "El cargo seleccionado no existe."
                });
            }

            // 3. Validar acto
            if (cargo.Acto == null || cargo.Acto.Eliminado)
            {
                return BadRequest(new
                {
                    Message = "El acto del cargo no está disponible."
                });
            }

            if (!cargo.Acto.EsActivo)
            {
                return BadRequest(new
                {
                    Message = "El acto no está habilitado para inscripciones."
                });
            }

            var ahora = DateHelper.ArgentinaNow();

            if (ahora < cargo.Acto.Inicio ||
                ahora > cargo.Acto.Fin)
            {
                return BadRequest(new
                {
                    Message = "El acto se encuentra fuera del período de inscripción."
                });
            }

            // 4. Validar relaciones del cargo
            if (cargo.Institucion == null ||
                cargo.Institucion.Eliminado)
            {
                return BadRequest(new
                {
                    Message = "La institución del cargo no está disponible."
                });
            }

            if (cargo.Orientacion == null ||
                cargo.Orientacion.Eliminado)
            {
                return BadRequest(new
                {
                    Message = "La orientación del cargo no está disponible."
                });
            }

            // 5. Evitar inscripción duplicada
            var existe = await _service.ExisteInscripcionAsync(
                cargo.Id, postulante.Id);

            if (existe)
            {
                return Conflict(new
                {
                    Message = "Ya estás inscripto en este cargo."
                });
            }

            decimal? puntaje = null;
            Caracter? caracter = null;

            // 6. Consultar API de Junta
            if (!string.IsNullOrWhiteSpace(cargo.CodigoJunta))
            {
                var codigo = cargo.CodigoJunta.Trim();

                // Normalizar localidad para Junta.
                // Ajustar Ciudad.Nombre si tu entidad usa Descripcion.
                var ciudad = ObtenerCodigoCiudad(
                    cargo.Institucion.Ciudad.Nombre);

                if (ciudad == null)
                {
                    return BadRequest(new
                    {
                        Message = "La localidad de la institución no está configurada para consultar Junta."
                    });
                }

                try
                {
                    var junta = await _juntaApiService
                        .ConsultarPuntajeAsync(
                            postulante.Dni,
                            codigo,
                            ciudad,
                            cancellationToken);

                    if (junta != null)
                    {
                        // Verificar correspondencia del DNI si viene informado
                        var dniRespuesta = junta.Dni?.Trim();

                        if (!string.IsNullOrWhiteSpace(dniRespuesta) &&
                            dniRespuesta != postulante.Dni.ToString(
                                CultureInfo.InvariantCulture))
                        {
                            _logger.LogWarning(
                                "Junta devolvió un DNI distinto al solicitado.");

                            junta = null;
                        }
                    }

                    if (junta != null)
                    {
                        if (decimal.TryParse(
                            junta.Puntaje,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var valorPuntaje))
                        {
                            puntaje = valorPuntaje;
                        }

                        if (!string.IsNullOrWhiteSpace(junta.Caracter) &&
                            Enum.TryParse<Caracter>(
                                junta.Caracter.Trim(),
                                true,
                                out var valorCaracter) &&
                            Enum.IsDefined(valorCaracter))
                        {
                            caracter = valorCaracter;
                        }
                    }
                }
                catch (OperationCanceledException)
                    when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Tiempo de espera agotado al consultar Junta.");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(
                        ex, "No se pudo consultar la API de Junta.");
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex, "Respuesta inválida de la API de Junta.");
                }
            }

            // 7. Crear inscripción
            var inscripcion = new Inscripcion
            {
                CargoId = cargo.Id,
                PostulanteId = postulante.Id,
                EstadoInscripcion = EstadoInscripcion.INSCRIPCION,
                FechaAsignacion = null,
                PuntajeJunta = puntaje,
                Caracter = caracter
            };

            try
            {
                /*
                 * Primero guardar la inscripción.
                 *
                 * A partir de este punto la inscripción
                 * ya existe en la base de datos.
                 */
                var result =
                    await _service.AddAsync(inscripcion);

                var emailEnviado = false;

                /*
                 * El correo NO forma parte de la operación
                 * principal.
                 *
                 * Si falla, la inscripción NO se revierte.
                 */
                try
                {
                    if (!string.IsNullOrWhiteSpace(
                        postulante.Email))
                    {
                        var frontendUrl =
                            _configuration["Frontend:Url"]
                            ?? throw new InvalidOperationException(
                                "Frontend:Url no está configurado.");

                        var logoUrl =
                            $"{frontendUrl.TrimEnd('/')}/logo.png";

                        var nombreCompleto =
                            $"{postulante.Nombres} {postulante.Apellidos}"
                                .Trim();

                        var actoDescripcion =
                            !string.IsNullOrWhiteSpace(
                                cargo.Acto.Descripcion)
                                ? $"{cargo.Acto.Numero} - {cargo.Acto.Descripcion}"
                                : cargo.Acto.Numero;

                        var body =
                            InscripcionConfirmacionEmailTemplate
                                .Generar(
                                    logoUrl: logoUrl,

                                    nombre:
                                        nombreCompleto,

                                    cargo:
                                        cargo.Descripcion,

                                    institucion:
                                        cargo.Institucion.Descripcion,

                                    acto:
                                        actoDescripcion,

                                    numeroLlamado:
                                        cargo.NumeroLLamado,

                                    curso:
                                        cargo.Curso,

                                    division:
                                        cargo.Division,

                                    turno:
                                        cargo.Turno.ToString(),

                                    revista:
                                        cargo.Revista.ToString(),

                                    puntaje:
                                        result.PuntajeJunta,

                                    caracter:
                                        result.Caracter?.ToString()
                                );

                        await _emailService.SendEmailAsync(
                            postulante.Email,
                            "Confirmación de inscripción | Actos Públicos Jóvenes y Adultos",
                            body);

                        emailEnviado = true;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "La inscripción {InscripcionId} fue creada, pero el postulante {PostulanteId} no tiene email.",
                            result.Id,
                            postulante.Id);
                    }
                }
                catch (Exception ex)
                {
                    /*
                     * IMPORTANTE:
                     * solamente registrar el error.
                     *
                     * NO lanzar nuevamente la excepción.
                     */
                    _logger.LogError(
                        ex,
                        "La inscripción {InscripcionId} fue creada correctamente, pero falló el envío del correo de confirmación.",
                        result.Id);
                }

                return StatusCode(
                    StatusCodes.Status201Created,
                    new
                    {
                        Message =
                            "Inscripción realizada correctamente.",

                        Inscripcion =
                            MapToResponse(result),

                        DatosJuntaDisponibles =
                            result.PuntajeJunta.HasValue ||
                            result.Caracter.HasValue,

                        EmailEnviado =
                            emailEnviado
                    });
            }
            catch (DbUpdateException ex)
                when (EsDuplicado(ex))
            {
                return Conflict(new
                {
                    Message = "Ya estás inscripto en este cargo."
                });
            }
        }

        private static string? ObtenerCodigoCiudad(string? nombre)
        {
            var normalizado = (nombre ?? "")
                .Trim()
                .ToUpperInvariant()
                .Replace("Í", "I");

            return normalizado switch
            {
                "RIO GRANDE" => "RGD",
                "RGD" => "RGD",
                "USHUAIA" => "USH",
                "USH" => "USH",
                "TOLHUIN" => "TOL",
                "TOL" => "TOL",
                _ => null
            };
        }

        private static bool EsDuplicado(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql &&
                (sql.Number == 2601 || sql.Number == 2627);
        }

        private static InscripcionResponseDto MapToResponse(
            Inscripcion inscripcion)
        {
            return new InscripcionResponseDto
            {
                Id = inscripcion.Id,
                CargoId = inscripcion.CargoId,
                PostulanteId = inscripcion.PostulanteId,
                EstadoInscripcion = inscripcion.EstadoInscripcion,
                PuntajeJunta = inscripcion.PuntajeJunta,
                Caracter = inscripcion.Caracter,
                FechaCreacion = inscripcion.FechaCreacion
            };
        }

        // MIS POSTULACIONES
        [HttpGet("MisPostulaciones")]
        [Authorize(Roles = "admin,docente")]
        public async Task<IActionResult> GetMisPostulaciones(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0)
            {
                return BadRequest(new
                {
                    Message = "La página debe ser mayor a cero."
                });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new
                {
                    Message = "El tamaño de página debe estar entre 1 y 100."
                });
            }

            var postulante = await _postulanteService
                .GetMiPostulanteAsync();

            if (postulante == null || postulante.Eliminado)
            {
                return NotFound(new
                {
                    Message = "No tenés datos de postulante registrados."
                });
            }

            var result = await _service.GetByPostulanteIdAsync(
                postulante.Id,
                page,
                pageSize);

            return Ok(new PagedResponse<InscripcionListadoDto>
            {
                Items = result.Items
                    .Select(MapToListadoDto)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        // POSTULACIONES DE UN CARGO - INSTITUCIÓN
        [HttpGet("Cargo/{cargoId:int}/Postulaciones")]
        [Authorize(Roles = "admin,institucion,docente")]
        public async Task<IActionResult> GetPostulacionesByCargo(
            int cargoId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (cargoId <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID del cargo debe ser mayor a cero."
                });
            }

            if (page <= 0)
            {
                return BadRequest(new
                {
                    Message = "La página debe ser mayor a cero."
                });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new
                {
                    Message = "El tamaño de página debe estar entre 1 y 100."
                });
            }            

            // Buscar cargo
            var cargo = await _service
                .GetCargoParaInscripcionAsync(cargoId);

            if (cargo == null || cargo.Eliminado)
            {
                return NotFound(new
                {
                    Message = "El cargo no existe."
                });
            }

            
            // ADMIN
            // Puede consultar cualquier cargo         
            if (User.IsInRole("admin"))
            {
                // No necesita validaciones adicionales.
            }            
            // INSTITUCIÓN
            // Solo puede consultar cargos propios           
            else if (User.IsInRole("institucion"))
            {
                var institucion = await _institucionService.GetMiInstitucionAsync();

                if (institucion == null || institucion.Eliminado)
                {
                    return Forbid();
                }

                if (cargo.InstitucionId != institucion.Id)
                {
                    return NotFound(new
                    {
                        Message = "El cargo no existe o no pertenece a tu institución."
                    });
                }
            }            
            // DOCENTE
            // Solo puede consultar si está inscripto           
            else if (User.IsInRole("docente"))
            {
                var postulante = await _postulanteService.GetMiPostulanteAsync();

                if (postulante == null || postulante.Eliminado)
                {
                    return Forbid();
                }

                var estaInscripto = await _service
                    .ExisteInscripcionAsync(
                        cargoId,
                        postulante.Id);

                if (!estaInscripto)
                {
                    return Forbid();
                }
            }

            var result = await _service.GetByCargoIdAsync(
                cargoId,
                page,
                pageSize);

            return Ok(new PagedResponse<InscripcionListadoDto>
            {
                Items = result.Items
                    .Select(MapToListadoDto)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        private static InscripcionListadoDto MapToListadoDto(Inscripcion inscripcion)
        {
            return new InscripcionListadoDto
            {
                Id = inscripcion.Id,

                EstadoInscripcion = inscripcion.EstadoInscripcion,
                FechaAsignacion = inscripcion.FechaAsignacion,

                PuntajeJunta = inscripcion.PuntajeJunta,
                Caracter = inscripcion.Caracter,

                FechaCreacion = inscripcion.FechaCreacion,

                Postulante = new PostulanteInscripcionDto
                {
                    Id = inscripcion.Postulante.Id,
                    Dni = inscripcion.Postulante.Dni,

                    Nombres = inscripcion.Postulante.Nombres,
                    Apellidos = inscripcion.Postulante.Apellidos,

                    NombreCompleto =
                        $"{inscripcion.Postulante.Apellidos}, " +
                        $"{inscripcion.Postulante.Nombres}",

                    Email = inscripcion.Postulante.Email,
                    Telefono = inscripcion.Postulante.Telefono,

                    Direccion = inscripcion.Postulante.Direccion,
                    Altura = inscripcion.Postulante.Altura,
                    Depto = inscripcion.Postulante.Depto,

                    CiudadId = inscripcion.Postulante.CiudadId,
                    CiudadNombre =
                        inscripcion.Postulante.Ciudad?.Nombre ?? ""
                },

                Cargo = new CargoInscripcionDto
                {
                    Id = inscripcion.Cargo.Id,

                    ActoId = inscripcion.Cargo.ActoId,
                    ActoNumero =
                        inscripcion.Cargo.Acto?.Numero ?? "",
                    ActoDescripcion =
                        inscripcion.Cargo.Acto?.Descripcion ?? "",

                    InstitucionId =
                        inscripcion.Cargo.InstitucionId,

                    InstitucionNombre =
                        inscripcion.Cargo.Institucion?.Descripcion ?? "",

                    OrientacionId =
                        inscripcion.Cargo.OrientacionId,

                    OrientacionDescripcion =
                        inscripcion.Cargo.Orientacion?.Descripcion ?? "",

                    Descripcion =
                        inscripcion.Cargo.Descripcion,

                    Codigo =
                        inscripcion.Cargo.Codigo,

                    NumeroLlamado =
                        inscripcion.Cargo.NumeroLLamado,

                    Turno =
                        inscripcion.Cargo.Turno,

                    Revista =
                        inscripcion.Cargo.Revista,

                    Curso =
                        inscripcion.Cargo.Curso,

                    Division =
                        inscripcion.Cargo.Division,

                    PlazaSUNA =
                        inscripcion.Cargo.PlazaSUNA,

                    CargaHoraria =
                        inscripcion.Cargo.CargaHoraria,

                    CodigoJunta =
                        inscripcion.Cargo.CodigoJunta
                }
            };
        }

        // ASIGNAR POSTULANTE A UN CARGO
        [HttpPut("Asignar")]
        [Authorize(Roles = "institucion")]
        public async Task<IActionResult> Asignar(
            [FromBody] AsignarInscripcionDto request)
        {
            if (request.InscripcionId <= 0)
            {
                return BadRequest(new
                {
                    Message =
                        "El ID de inscripción debe ser mayor a cero."
                });
            }

            // Obtener institución del usuario autenticado
            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (
                institucion == null ||
                institucion.Eliminado
            )
            {
                return Forbid();
            }

            // Buscar inscripción
            var inscripcion = await _service
                .GetByIdParaAsignacionAsync(
                    request.InscripcionId);

            if (inscripcion == null)
            {
                return NotFound(new
                {
                    Message = "La inscripción no existe."
                });
            }

            // Validar cargo
            if (
                inscripcion.Cargo == null ||
                inscripcion.Cargo.Eliminado
            )
            {
                return NotFound(new
                {
                    Message = "El cargo no existe."
                });
            }

            // La institución solamente puede asignar
            // postulantes de sus propios cargos.
            if (
                inscripcion.Cargo.InstitucionId !=
                institucion.Id
            )
            {
                return NotFound(new
                {
                    Message =
                        "La inscripción no pertenece a un cargo de tu institución."
                });
            }

            // Ya está asignado
            if (
                inscripcion.EstadoInscripcion ==
                EstadoInscripcion.ASIGNADO
            )
            {
                return Conflict(new
                {
                    Message =
                        "Este postulante ya se encuentra asignado al cargo."
                });
            }

            // Comprobar si otro postulante ya fue asignado
            var existeAsignado = await _service
                .ExisteAsignadoEnCargoAsync(
                    inscripcion.CargoId);

            if (existeAsignado)
            {
                return Conflict(new
                {
                    Message =
                        "El cargo ya tiene un postulante asignado."
                });
            }

            try
            {
                var result = await _service.AsignarAsync(
                    inscripcion.Id,
                    institucion.Id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message =
                            "No se pudo encontrar la inscripción."
                    });
                }

                return Ok(new
                {
                    Message =
                        "Postulante asignado correctamente.",

                    Inscripcion =
                        new AsignarInscripcionResponseDto
                        {
                            Id = result.Id,
                            CargoId = result.CargoId,
                            PostulanteId =
                                result.PostulanteId,

                            EstadoInscripcion =
                                result.EstadoInscripcion,

                            FechaAsignacion =
                                result.FechaAsignacion
                        }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Message = ex.Message
                });
            }
            catch (DbUpdateException ex)
                when (EsDuplicado(ex))
            {
                return Conflict(new
                {
                    Message =
                        "El cargo ya tiene un postulante asignado."
                });
            }
        }

        // QUITAR ASIGNACIÓN
        [HttpPut("QuitarAsignacion/{inscripcionId:int}")]
        [Authorize(Roles = "institucion")]
        public async Task<IActionResult> QuitarAsignacion(
            int inscripcionId)
        {
            if (inscripcionId <= 0)
            {
                return BadRequest(new
                {
                    Message =
                        "El ID de inscripción debe ser mayor a cero."
                });
            }

            // Institución del usuario autenticado
            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (
                institucion == null ||
                institucion.Eliminado
            )
            {
                return Forbid();
            }

            // Obtener inscripción
            var inscripcion = await _service
                .GetByIdParaAsignacionAsync(
                    inscripcionId);

            if (inscripcion == null)
            {
                return NotFound(new
                {
                    Message =
                        "La inscripción no existe."
                });
            }

            // Validar cargo
            if (
                inscripcion.Cargo == null ||
                inscripcion.Cargo.Eliminado
            )
            {
                return NotFound(new
                {
                    Message =
                        "El cargo de la inscripción no existe."
                });
            }

            // Validar que sea un cargo de esta institución
            if (
                inscripcion.Cargo.InstitucionId !=
                institucion.Id
            )
            {
                return NotFound(new
                {
                    Message =
                        "La inscripción no pertenece a un cargo de tu institución."
                });
            }

            // Debe estar asignada
            if (
                inscripcion.EstadoInscripcion !=
                EstadoInscripcion.ASIGNADO
            )
            {
                return Conflict(new
                {
                    Message =
                        "La inscripción no se encuentra asignada."
                });
            }

            try
            {
                var result = await _service
                    .QuitarAsignacionAsync(
                        inscripcionId,
                        institucion.Id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message =
                            "No se pudo encontrar la inscripción."
                    });
                }

                return Ok(new
                {
                    Message =
                        "Asignación quitada correctamente.",

                    Inscripcion =
                        new AsignarInscripcionResponseDto
                        {
                            Id = result.Id,
                            CargoId = result.CargoId,
                            PostulanteId =
                                result.PostulanteId,

                            EstadoInscripcion =
                                result.EstadoInscripcion,

                            FechaAsignacion =
                                result.FechaAsignacion
                        }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Message = ex.Message
                });
            }
        }

        // PDF RESULTADO DEL CARGO
        [HttpGet("Cargo/{cargoId:int}/Pdf")]
        [Authorize(Roles = "institucion")]
        public async Task<IActionResult> GetCargoPdf(
            int cargoId)
        {
            if (cargoId <= 0)
            {
                return BadRequest(new
                {
                    Message =
                        "El ID del cargo debe ser mayor a cero."
                });
            }

            // 1. Obtener institución autenticada
            var institucion =
                await _institucionService
                    .GetMiInstitucionAsync();

            if (
                institucion == null ||
                institucion.Eliminado
            )
            {
                return Forbid();
            }

            // 2. Obtener cargo
            var cargo =
                await _service
                    .GetCargoParaInscripcionAsync(
                        cargoId);

            if (
                cargo == null ||
                cargo.Eliminado
            )
            {
                return NotFound(new
                {
                    Message =
                        "El cargo no existe."
                });
            }

            // 3. Validar que pertenezca
            // a la institución autenticada
            if (
                cargo.InstitucionId !=
                institucion.Id
            )
            {
                return NotFound(new
                {
                    Message =
                        "El cargo no existe o no pertenece a tu institución."
                });
            }

            // 4. Obtener todos los datos
            var datos =
                await _service
                    .GetDatosAsignacionPdfAsync(
                        cargoId);

            if (datos == null)
            {
                return NotFound(new
                {
                    Message =
                        "No se encontraron datos para el cargo."
                });
            }

            // 5. Debe existir un asignado
            var existeAsignado =
                datos.Inscriptos.Any(i =>
                    i.EsAsignado);

            if (!existeAsignado)
            {
                return Conflict(new
                {
                    Message =
                        "No se puede generar el PDF porque el cargo todavía no tiene un postulante asignado."
                });
            }

            // 6. Generar PDF
            var pdf =
                _pdfService.GenerarPdf(
                    datos);

            var fileName =
                $"resultado-cargo-{cargoId}.pdf";

            return File(
                pdf,
                "application/pdf",
                fileName);
        }
    }
}
