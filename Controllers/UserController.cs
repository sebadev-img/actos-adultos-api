using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        // OBTENER USUARIOS PAGINADOS
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] UserQueryRequest request)
        {
            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message = "La búsqueda no puede superar los 100 caracteres."
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Role) &&
                !await _service.RoleExistsAsync(request.Role.Trim()))
            {
                return BadRequest(new
                {
                    Message = "El rol seleccionado no existe."
                });
            }

            var result = await _service.GetPagedAsync(request);

            return Ok(result);
        }

        // CAMBIAR ROL DE USUARIO
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRole(
            string id,
            [FromBody] ChangeUserRoleDto request)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    Message = "El ID del usuario es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new
                {
                    Message = "Debe seleccionar un rol."
                });
            }

            var newRole = request.Role.Trim().ToLowerInvariant();

            // Roles permitidos en la aplicación
            string[] allowedRoles =
            {
            "admin",
            "docente",
            "institucion"
        };

            if (!allowedRoles.Contains(newRole))
            {
                return BadRequest(new
                {
                    Message = "El rol seleccionado no está permitido."
                });
            }

            // Buscar usuario
            var user = await _service.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    Message = "El usuario no existe."
                });
            }

            // Obtener roles actuales
            var currentRoles = await _service.GetRolesAsync(user);

            // No permitir modificar el propio rol admin
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == user.Id)
            {
                return BadRequest(new
                {
                    Message = "No podés modificar tu propio rol."
                });
            }

            // Evitar quitar el rol institucion a un usuario
            // que todavía tiene institución asociada.
            if (currentRoles.Contains(
                    "institucion",
                    StringComparer.OrdinalIgnoreCase) &&
                newRole != "institucion")
            {
                var tieneInstitucion =
                    await _service.HasInstitucionAsync(user.Id);

                if (tieneInstitucion)
                {
                    return Conflict(new
                    {
                        Message = "El usuario tiene una institución asignada. Primero debés quitar esa asignación."
                    });
                }
            }

            // Verificar que exista el rol
            if (!await _service.RoleExistsAsync(newRole))
            {
                return BadRequest(new
                {
                    Message = "El rol seleccionado no existe en el sistema."
                });
            }

            // Cambiar rol
            var result = await _service.ChangeRoleAsync(
                user, newRole);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "No se pudo cambiar el rol.",
                    Errors = result.Errors
                        .Select(e => e.Description)
                });
            }

            return Ok(new
            {
                Message = "Rol actualizado correctamente.",
                UserId = user.Id,
                Role = newRole
            });
        }
    }
}
