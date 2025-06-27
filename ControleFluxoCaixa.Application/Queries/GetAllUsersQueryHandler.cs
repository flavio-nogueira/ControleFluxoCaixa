using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Handler responsável por tratar a requisição que retorna todos os usuários do sistema.
    /// Utiliza cache para evitar chamadas desnecessárias ao banco e melhorar o desempenho.
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager; // Gerenciador de usuários do Identity
        private readonly ICacheService _cache; // Serviço de cache (Redis, Memcached, etc.)
        private readonly ILogger<GetAllUsersQueryHandler> _logger; // Serviço de log

        /// <summary>
        /// Construtor que injeta dependências necessárias.
        /// </summary>
        public GetAllUsersQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICacheService cache,
            ILogger<GetAllUsersQueryHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Manipula a query para retornar todos os usuários.
        /// Se houver cache, utiliza o valor em cache.
        /// Caso contrário, busca do banco, armazena no cache e retorna.
        /// </summary>
        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            const string key = "users:all"; // Chave utilizada no cache

            // Busca os dados no cache ou gera e armazena por 5 minutos
            var usuarios = await _cache.GetOrSetAsync(
                key,
                () => Task.FromResult(
                    _userManager.Users
                        .Select(u => new UserDto(u.Id.ToString(), u.Email, u.FullName))
                        .ToList()
                ),
                TimeSpan.FromMinutes(5),
                cancellationToken
            );

            // Loga a quantidade de usuários retornados
            _logger.LogInformation("Consulta de todos os usuários retornada com sucesso ({Count})", usuarios.Count);

            return usuarios;
        }
    }
}
