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
        private readonly ICacheService _cache;
        private readonly ILogger<LoginCommandHandler> _logger;

        // Construtor com injeção de dependências necessárias para o login
        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenSvc,
            IRefreshTokenService rtSvc,
            ICacheService cache,
            ILogger<LoginCommandHandler> logger)
        {
            _userManager = userManager;
            _tokenSvc = tokenSvc;
            _rtSvc = rtSvc;
            _cache = cache;
            _logger = logger;
        }

        // Manipula a requisição de login
        public async Task<RefreshDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Loga o início do processo de login
            _logger.LogInformation("Iniciando processo de login para o e-mail {Email}", request.Email);

            // Busca o usuário pelo e-mail
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Valida se o usuário existe e se a senha está correta
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Login falhou para o e-mail {Email}", request.Email);
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }

            // Gera um access token JWT
            var jwt = await _tokenSvc.GenerateAccessTokenAsync(user);

            // Gera um refresh token vinculado ao IP do cliente
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(user, request.IpAddress);

            // Define a chave de cache para registrar a quantidade de logins do usuário
            var key = $"logins:{user.Id}";

            // Recupera ou define o contador de logins (com duração de 1 hora)
            var count = await _cache.GetOrSetAsync(key, async () => 0, TimeSpan.FromHours(1), cancellationToken);

            // Remove o valor atual do cache (opcional, forçando regravação)
            await _cache.RemoveAsync(key, cancellationToken);

            // Atualiza o contador de logins para o próximo valor
            await _cache.GetOrSetAsync(key, () => Task.FromResult(count + 1), TimeSpan.FromHours(1), cancellationToken);

            // Loga sucesso no login com o número da tentativa
            _logger.LogInformation("Login bem-sucedido para o usuário {UserId}, login número {Count} registrado.", user.Id, count + 1);

            // Retorna DTO contendo access token e refresh token
            return new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            };
        }
    }
}
