using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Commands.Auth.RefreshToken
{
    /// <summary>
    /// Handler responsável por processar a renovação do JWT com base em um refresh token válido.
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshDto>
    {
        // Serviço responsável por validar e gerar novos refresh tokens
        private readonly IRefreshTokenService _rtSvc;

        // Serviço responsável por gerar novos tokens JWT
        private readonly ITokenService _tokenSvc;

        // Serviço de cache genérico (implementado via Redis, MemoryCache ou outro)
        private readonly ICacheService _cache;

        // Acesso ao contexto HTTP para obter o IP do usuário
        private readonly IHttpContextAccessor _http;

        // Logger para registrar informações, alertas e erros
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        /// <summary>
        /// Construtor do handler com injeção de dependências.
        /// </summary>
        public RefreshTokenCommandHandler(
            IRefreshTokenService rtSvc,
            ITokenService tokenSvc,
            ICacheService cache,
            IHttpContextAccessor http,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _rtSvc = rtSvc;
            _tokenSvc = tokenSvc;
            _cache = cache;
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Manipula a requisição de renovação de token com base no RefreshToken recebido.
        /// </summary>
        public async Task<RefreshDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Chave que representa a blacklist do refresh token no cache
            var blKey = $"rt_blacklist:{request.RefreshToken}";

            // Tenta obter a chave do cache para verificar se o token já foi usado (blacklist)
            var isBlacklisted = await _cache.GetOrSetAsync<string?>(
                key: blKey,
                factory: () => Task.FromResult<string?>(null), // Se não houver valor, retorna nulo
                duration: TimeSpan.FromSeconds(10),            // TTL curto para garantir checagem frequente
                cancellationToken: cancellationToken
            );

            // Se o token estiver na blacklist, rejeita a renovação
            if (isBlacklisted != null)
            {
                _logger.LogWarning("Refresh token já foi usado: {Token}", request.RefreshToken);
                throw new UnauthorizedAccessException("Refresh token já consumido.");
            }

            // Valida o refresh token e marca como consumido no banco
            var validation = await _rtSvc.ValidateAndConsumeRefreshTokenAsync(request.RefreshToken);

            // Se o token estiver inválido ou o usuário não for encontrado, rejeita
            if (!validation.IsValid || validation.User == null)
            {
                _logger.LogWarning("Refresh token inválido ou expirado: {Token}", request.RefreshToken);
                throw new UnauthorizedAccessException("RefreshToken inválido ou expirado.");
            }

            // Após a validação, marca o token como usado no cache por 7 dias
            await _cache.GetOrSetAsync(
                key: blKey,
                factory: () => Task.FromResult("used"),        // Valor fixo indicando "usado"
                duration: TimeSpan.FromDays(7),                // TTL longo para evitar reuso
                cancellationToken: cancellationToken
            );

            // Captura o IP do usuário da requisição HTTP (pode ser usado no log ou geração do token)
            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Gera um novo JWT para o usuário autenticado
            var jwt = await _tokenSvc.GenerateAccessTokenAsync(validation.User);

            // Gera um novo refresh token vinculado ao IP atual
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(validation.User, ip);

            // Loga a renovação do token com sucesso
            _logger.LogInformation("Refresh token renovado para o usuário {UserId}", validation.User.Id);

            // Retorna o novo par de tokens
            return new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            };
        }
    }
}
