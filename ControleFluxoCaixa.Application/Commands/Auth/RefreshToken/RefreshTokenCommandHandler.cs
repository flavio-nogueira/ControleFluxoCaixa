using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Commands.Auth.RefreshToken
{
    /// <summary>
    /// Handler que processa a geração de novo JWT com base em um refresh token ainda válido.
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshDto>
    {
        private readonly IRefreshTokenService _rtSvc;
        private readonly ITokenService _tokenSvc;
        private readonly IGenericCacheService _cache;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IRefreshTokenService rtSvc,
            ITokenService tokenSvc,
            IGenericCacheService cache,
            IHttpContextAccessor http,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _rtSvc = rtSvc;
            _tokenSvc = tokenSvc;
            _cache = cache;
            _http = http;
            _logger = logger;
        }

        public async Task<RefreshDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var blKey = $"rt_blacklist:{request.RefreshToken}";
            var isBlacklisted = await _cache.GetOrSetAsync<string?>(blKey, () => Task.FromResult<string?>(null), TimeSpan.Zero, cancellationToken);
            if (isBlacklisted != null)
            {
                _logger.LogWarning("Refresh token já foi usado: {Token}", request.RefreshToken);
                throw new UnauthorizedAccessException("Refresh token já consumido.");
            }

            var validation = await _rtSvc.ValidateAndConsumeRefreshTokenAsync(request.RefreshToken);
            if (!validation.IsValid || validation.User == null)
            {
                _logger.LogWarning("Refresh token inválido ou expirado: {Token}", request.RefreshToken);
                throw new UnauthorizedAccessException("RefreshToken inválido ou expirado.");
            }

            await _cache.GetOrSetAsync(blKey, () => Task.FromResult("used"), TimeSpan.FromDays(7), cancellationToken);

            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var jwt = await _tokenSvc.GenerateAccessTokenAsync(validation.User);
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(validation.User, ip);

            _logger.LogInformation("Refresh token renovado para o usuário {UserId}", validation.User.Id);

            return new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            };
        }
    }
}
