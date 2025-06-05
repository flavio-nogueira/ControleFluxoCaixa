using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Commands.Auth.Login
{
    /// <summary>
    /// Handler que executa o processo de login: valida credenciais, gera tokens e registra a tentativa em cache.
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, RefreshDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenSvc;
        private readonly IRefreshTokenService _rtSvc;
        private readonly IGenericCacheService _cache;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenSvc,
            IRefreshTokenService rtSvc,
            IGenericCacheService cache,
            ILogger<LoginCommandHandler> logger)
        {
            _userManager = userManager;
            _tokenSvc = tokenSvc;
            _rtSvc = rtSvc;
            _cache = cache;
            _logger = logger;
        }

        public async Task<RefreshDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando processo de login para o e-mail {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Login falhou para o e-mail {Email}", request.Email);
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }

            var jwt = await _tokenSvc.GenerateAccessTokenAsync(user);
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(user, request.IpAddress);

            var key = $"logins:{user.Id}";
            var count = await _cache.GetOrSetAsync(key, async () => 0, TimeSpan.FromHours(1), cancellationToken);
            await _cache.RemoveAsync(key, cancellationToken);
            await _cache.GetOrSetAsync(key, () => Task.FromResult(count + 1), TimeSpan.FromHours(1), cancellationToken);

            _logger.LogInformation("Login bem-sucedido para o usuário {UserId}, login número {Count} registrado.", user.Id, count + 1);

            return new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            };
        }
    }
}
