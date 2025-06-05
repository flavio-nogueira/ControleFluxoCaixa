using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Queries.Auth.GetUserById
{
    /// <summary>
    /// Handler que retorna os dados de um usuário identificado pelo seu ID, utilizando cache para melhorar a performance.
    /// </summary>
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericCacheService _cache;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(
            UserManager<ApplicationUser> userManager,
            IGenericCacheService cache,
            ILogger<GetUserByIdQueryHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var key = $"user:{request.Id}";

            var dto = await _cache.GetOrSetAsync(key, async () =>
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null) return null;
                return new UserDto(user.Id.ToString(), user.Email, user.FullName);
            },
            TimeSpan.FromMinutes(5), cancellationToken);

            if (dto != null)
                _logger.LogInformation("Usuário {Id} encontrado com sucesso.", request.Id);
            else
                _logger.LogWarning("Usuário {Id} não encontrado.", request.Id);

            return dto;
        }
    }
}
