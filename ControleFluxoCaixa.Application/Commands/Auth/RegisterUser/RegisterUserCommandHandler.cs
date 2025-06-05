using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Commands.Auth.RegisterUser
{
    /// <summary>
    /// Handler que executa o processo de registro de um novo usuário, incluindo invalidação de cache.
    /// </summary>
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericCacheService _cache;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IGenericCacheService cache,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                _logger.LogWarning("Tentativa de registro com e-mail já existente: {Email}", request.Email);
                throw new InvalidOperationException("E-mail já cadastrado.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName
            };

            var res = await _userManager.CreateAsync(user, request.Password);
            if (!res.Succeeded)
            {
                _logger.LogError("Erro ao registrar usuário: {Erros}", res.Errors);
                throw new Exception("Falha ao criar usuário.");
            }

            await _cache.RemoveAsync("users:all", cancellationToken);

            _logger.LogInformation("Novo usuário registrado com sucesso: {UserId}", user.Id);

            return new UserDto(user.Id.ToString(), user.Email, user.FullName);
        }
    }
}
