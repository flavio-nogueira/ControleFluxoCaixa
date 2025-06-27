using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Handler que retorna os dados de um usuário identificado pelo seu ID, utilizando cache para melhorar a performance.
    /// </summary>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly UserManager<ApplicationUser> _userManager; // Gerenciador de usuários do Identity
        private readonly ICacheService _cache;                      // Serviço de cache para reduzir consultas ao banco
        private readonly ILogger<GetUserByIdQueryHandler> _logger; // Logger para registrar informações

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        public GetUserByIdQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICacheService cache,
            ILogger<GetUserByIdQueryHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Manipulador da query que busca um usuário pelo ID, utilizando cache.
        /// </summary>
        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            // Chave do cache para este usuário
            var key = $"user:{request.Id}";

            // Tenta obter do cache, senão busca no banco e salva no cache por 5 minutos
            var dto = await _cache.GetOrSetAsync(key, async () =>
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null) return null;

                // Mapeia o usuário encontrado para o DTO
                return new UserDto(user.Id.ToString(), user.Email, user.FullName);
            },
            TimeSpan.FromMinutes(5), cancellationToken);

            // Loga se encontrou ou não
            if (dto != null)
                _logger.LogInformation("Usuário {Id} encontrado com sucesso.", request.Id);
            else
                _logger.LogWarning("Usuário {Id} não encontrado.", request.Id);

            return dto;
        }
    }
}
