using ActosAdultosAPI.Data;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Services.Implementation
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private int GetExpirationDays()
        {
            var days = _configuration.GetValue<int>(
                "Jwt:RefreshTokenExpirationDays", 7);

            if (days <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del Refresh Token es inválida.");
            }

            return days;
        }

        // CREAR REFRESH TOKEN
        public async Task<RefreshTokenResult> CreateAsync(
            string usuarioId)
        {
            var now = DateTime.UtcNow;
            var token = GenerateToken();
            var expiresAt = now.AddDays(GetExpirationDays());

            var entity = new RefreshToken
            {
                UsuarioId = usuarioId,
                TokenHash = HashToken(token),
                FechaCreacion = now,
                FechaExpiracion = expiresAt
            };

            _context.RefreshTokens.Add(entity);
            await _context.SaveChangesAsync();

            return new RefreshTokenResult(
                token,
                usuarioId,
                expiresAt);
        }

        // RENOVAR REFRESH TOKEN
        public async Task<RefreshTokenResult?> RotateAsync(
            string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var now = DateTime.UtcNow;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var existing = await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TokenHash == hash);

            if (existing == null ||
                existing.FechaRevocacion != null ||
                existing.FechaExpiracion <= now)
            {
                return null;
            }

            // Revocación condicional y atómica.
            // Solamente una solicitud puede consumir el token.
            var affected = await _context.RefreshTokens
                .Where(x =>
                    x.Id == existing.Id &&
                    x.FechaRevocacion == null &&
                    x.FechaExpiracion > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.FechaRevocacion,
                        (DateTime?)now));

            if (affected != 1)
                return null;

            var newToken = GenerateToken();
            var newExpiresAt = now.AddDays(GetExpirationDays());

            var newEntity = new RefreshToken
            {
                UsuarioId = existing.UsuarioId,
                TokenHash = HashToken(newToken),
                FechaCreacion = now,
                FechaExpiracion = newExpiresAt
            };

            _context.RefreshTokens.Add(newEntity);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new RefreshTokenResult(
                newToken,
                existing.UsuarioId,
                newExpiresAt);
        }

        // REVOCAR REFRESH TOKEN
        public async Task RevokeAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var now = DateTime.UtcNow;

            await _context.RefreshTokens
                .Where(x =>
                    x.TokenHash == hash &&
                    x.FechaRevocacion == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.FechaRevocacion,
                        (DateTime?)now));
        }
    }
}
