namespace ActosAdultosAPI.Services.Interfaces
{
    public record RefreshTokenResult(
        string Token,
        string UsuarioId,
        DateTime ExpiresAt);

    public interface IRefreshTokenService
    {
        Task<RefreshTokenResult> CreateAsync(string usuarioId);

        Task<RefreshTokenResult?> RotateAsync(string refreshToken);

        Task RevokeAsync(string refreshToken);
    }
}
