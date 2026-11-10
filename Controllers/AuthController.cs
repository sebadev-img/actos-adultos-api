using ActosAdultosAPI.DTO;
using ActosAdultosAPI.EmailTemplates;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        // REGISTRAR USUARIO
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto request)
        {
            var email = request.Email.Trim();

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    Message = "El email ya está registrado."
                });
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(
                user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "No se pudo registrar el usuario.",
                    Errors = result.Errors
                        .Select(e => e.Description)
                });
            }

            // Asignar rol docente por defecto
            var roleResult = await _userManager.AddToRoleAsync(
                user, "docente");

            if (!roleResult.Succeeded)
            {
                // Evitar dejar un usuario creado sin el rol obligatorio
                var deleteResult = await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    _logger.LogError(
                        "No se pudo revertir el registro del usuario {UserId}",
                        user.Id);
                }

                return StatusCode(500, new
                {
                    Message = "No se pudo asignar el rol docente."
                });
            }

            try
            {
                await SendConfirmationEmailAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error enviando confirmación al usuario {UserId}",
                    user.Id);

                // El usuario ya está registrado.
                // Puede solicitar otro email de confirmación.
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        Message = "El usuario fue registrado, pero no se pudo enviar el correo de confirmación. Intentá reenviarlo."
                    });
            }

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    Message = "Usuario registrado correctamente. Revisá tu correo para activar la cuenta."
                });
        }

        // INICIAR SESIÓN
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email.Trim());

            // Email incorrecto
            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "El email ingresado no está registrado.",
                    Code = "INVALID_EMAIL"
                });
            }

            // Contraseña incorrecta
            // CheckPasswordAsync no incrementa
            // los intentos fallidos de Identity.
            var validPassword =
                await _userManager.CheckPasswordAsync(
                    user, request.Password);

            if (!validPassword)
            {
                return Unauthorized(new
                {
                    Message = "La contraseña ingresada es incorrecta.",
                    Code = "INVALID_PASSWORD"
                });
            }

            // Email todavía no confirmado
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new
                {
                    Message = "Debés confirmar tu correo electrónico antes de iniciar sesión.",
                    Code = "EMAIL_NOT_CONFIRMED"
                });
            }

            // Sin bloqueo por credenciales incorrectas.
            // Si la cuenta fue bloqueada administrativamente,
            // se respeta ese bloqueo.
            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized(new
                {
                    Message = "La cuenta se encuentra bloqueada.",
                    Code = "ACCOUNT_LOCKED"
                });
            }

            var (token, expiresAt) = await _jwtService.GenerateTokenAsync(user);

            var refreshToken = await _refreshTokenService.CreateAsync(user.Id);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,

                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt,

                UserId = user.Id,
                Email = user.Email ?? "",
                Roles = roles
            });
        }


        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken( [FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new
                {
                    Message = "El Refresh Token es obligatorio."
                });
            }

            var result = await _refreshTokenService.RotateAsync(
                request.RefreshToken);

            if (result == null)
            {
                return Unauthorized(new
                {
                    Message = "El Refresh Token es inválido, expiró o fue revocado.",
                    Code = "INVALID_REFRESH_TOKEN"
                });
            }

            var user = await _userManager.FindByIdAsync(
                result.UsuarioId);

            if (user == null ||
                !await _userManager.IsEmailConfirmedAsync(user) ||
                await _userManager.IsLockedOutAsync(user))
            {
                // Revocar el nuevo token si la cuenta
                // ya no está habilitada para iniciar sesión.
                await _refreshTokenService.RevokeAsync(result.Token);

                return Unauthorized(new
                {
                    Message = "La sesión ya no está autorizada.",
                    Code = "SESSION_NOT_AUTHORIZED"
                });
            }

            var (token, expiresAt) =
                await _jwtService.GenerateTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,

                RefreshToken = result.Token,
                RefreshTokenExpiresAt = result.ExpiresAt,

                UserId = user.Id,
                Email = user.Email ?? "",
                Roles = roles
            });
        }


        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout( [FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new
                {
                    Message = "El Refresh Token es obligatorio."
                });
            }

            await _refreshTokenService.RevokeAsync(
                request.RefreshToken);

            return Ok(new
            {
                Message = "Sesión cerrada correctamente."
            });
        }



        // CONFIRMAR EMAIL
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new
                {
                    Message = "Enlace de confirmación inválido."
                });
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return Ok(new
                {
                    Message = "El correo ya está confirmado."
                });
            }

            var result = await _userManager.ConfirmEmailAsync(
                user, token);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "El enlace de confirmación es inválido o expiró."
                });
            }

            return Ok(new
            {
                Message = "Correo confirmado correctamente. Ya podés iniciar sesión."
            });
        }

        // REENVIAR CONFIRMACIÓN
        [HttpPost("resend-confirmation-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationEmail(
            [FromBody] ResendConfirmationEmailDto request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email.Trim());

            // Respuesta genérica para no revelar
            // si la cuenta existe.
            var response = new
            {
                Message = "Si existe una cuenta pendiente de confirmación, recibirás un nuevo correo."
            };

            if (user == null ||
                await _userManager.IsEmailConfirmedAsync(user))
            {
                return Ok(response);
            }

            try
            {
                await SendConfirmationEmailAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error reenviando confirmación al usuario {UserId}",
                    user.Id);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        Message = "No se pudo procesar el envío. Intentá nuevamente más tarde."
                    });
            }

            return Ok(response);
        }

        // SOLICITAR RECUPERACIÓN DE CONTRASEÑA
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto request)
        {
            /*
             * Siempre se devuelve la misma respuesta para no
             * revelar si un email está registrado o no.
             */
            var response = new
            {
                Message =
                    "Si existe una cuenta asociada al correo ingresado, recibirás un enlace para restablecer tu contraseña."
            };

            var email = request.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Ok(response);
            }

            /*
             * Si todavía no confirmó el correo, tampoco
             * revelamos esa información.
             *
             * En ese caso debe utilizar el reenvío
             * de confirmación de correo.
             */
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Ok(response);
            }

            try
            {
                var frontendUrl =
                    _configuration["Frontend:Url"]
                    ?? throw new InvalidOperationException(
                        "Frontend:Url no está configurado.");

                frontendUrl =
                    frontendUrl.TrimEnd('/');

                // Token generado por Identity
                var token =
                    await _userManager
                        .GeneratePasswordResetTokenAsync(
                            user);

                // Página Angular
                var resetUrl =
                    $"{frontendUrl}/reset-password" +
                    $"?userId={Uri.EscapeDataString(user.Id)}" +
                    $"&token={Uri.EscapeDataString(token)}";

                var logoUrl =
                    $"{frontendUrl}/logo.png";

                var body =
                    RecuperarPasswordEmailTemplate.Generar(
                        resetUrl,
                        logoUrl);

                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Restablecer contraseña | Actos Públicos Jóvenes y Adultos",
                    body);
            }
            catch (Exception ex)
            {
                /*
                 * Se registra internamente pero no se informa
                 * si la cuenta realmente existe.
                 */
                _logger.LogError(
                    ex,
                    "Error enviando recuperación de contraseña al usuario {UserId}",
                    user.Id);
            }

            return Ok(response);
        }

        // RESTABLECER CONTRASEÑA
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new
                {
                    Message =
                        "El enlace para restablecer la contraseña no es válido."
                });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new
                {
                    Message =
                        "La nueva contraseña es obligatoria."
                });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new
                {
                    Message =
                        "Las contraseñas no coinciden."
                });
            }

            var user =
                await _userManager.FindByIdAsync(
                    request.UserId);

            if (user == null)
            {
                return BadRequest(new
                {
                    Message =
                        "El enlace para restablecer la contraseña es inválido o expiró."
                });
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    request.Token,
                    request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message =
                        "No se pudo restablecer la contraseña. El enlace puede ser inválido o haber expirado.",

                    Errors = result.Errors
                        .Select(e => e.Description)
                        .ToList()
                });
            }

            return Ok(new
            {
                Message =
                    "La contraseña fue modificada correctamente. Ya podés iniciar sesión con tu nueva contraseña."
            });
        }

        // USUARIO AUTENTICADO
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                Roles = roles
            });
        }

        // MÉTODO PRIVADO PARA ENVIAR CONFIRMACIÓN        
        private async Task SendConfirmationEmailAsync(
            AppUser user)
        {
            // URL del frontend
            var frontendUrl =
                _configuration["Frontend:Url"]
                ?? throw new InvalidOperationException(
                    "Frontend:Url no está configurado.");

            frontendUrl =
                frontendUrl.TrimEnd('/');

            // Generar token de confirmación
            var token =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(
                        user);

            // URL hacia Angular para confirmar email
            var confirmationUrl =
                $"{frontendUrl}/confirm-email" +
                $"?userId={Uri.EscapeDataString(user.Id)}" +
                $"&token={Uri.EscapeDataString(token)}";

            // Logo público del frontend
            var logoUrl =
                $"{frontendUrl}/logo.png";

            // Generar HTML
            var body =
                ConfirmacionEmailTemplate.Generar(
                    confirmationUrl,
                    logoUrl);

            await _emailService.SendEmailAsync(
                user.Email!,
                "Confirmá tu correo | Actos Públicos Jóvenes y Adultos",
                body);
        }
    }
}
