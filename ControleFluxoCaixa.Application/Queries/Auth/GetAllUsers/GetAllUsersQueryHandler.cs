using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Queries.Auth.GetAllUsers
{
    /// <summary>
    /// Handler que retorna todos os usuários do sistema, utilizando cache para otimizar o desempenho.
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericCacheService _cache;
        private readonly ILogger<GetAllUsersQueryHandler> _logger;

        public GetAllUsersQueryHandler(
            UserManager<ApplicationUser> userManager,
            IGenericCacheService cache,
            ILogger<GetAllUsersQueryHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            const string key = "users:all";

            var usuarios = await _cache.GetOrSetAsync(key, () => Task.FromResult(
                _userManager.Users.Select(u =>
                    new UserDto(u.Id.ToString(), u.Email, u.FullName)).ToList()),
                TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogInformation("Consulta de todos os usuários retornada com sucesso ({Count})", usuarios.Count);
            return usuarios;
        }
    }
}
