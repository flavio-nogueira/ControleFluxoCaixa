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
        private readonly ICacheService _cache;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        // Construtor com injeção das dependências: gerenciador de usuários, serviço de cache e logger
        public RegisterUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICacheService cache,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        // Manipulador do comando de registro de usuário
        public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Verifica se o e-mail já está cadastrado
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                _logger.LogWarning("Tentativa de registro com e-mail já existente: {Email}", request.Email);
                throw new InvalidOperationException("E-mail já cadastrado.");
            }

            // Cria nova instância do usuário com os dados informados
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName
            };

            // Tenta registrar o usuário no sistema com a senha informada
            var res = await _userManager.CreateAsync(user, request.Password);

            // Verifica se houve erro na criação do usuário
            if (!res.Succeeded)
            {
                _logger.LogError("Erro ao registrar usuário: {Erros}", res.Errors);
                throw new Exception("Falha ao criar usuário.");
            }

            // Remove o cache global de usuários para forçar atualização em futuras consultas
            await _cache.RemoveAsync("users:all", cancellationToken);

            // Loga o sucesso do registro
            _logger.LogInformation("Novo usuário registrado com sucesso: {UserId}", user.Id);

            // Retorna os dados básicos do usuário registrado (DTO)
            return new UserDto(user.Id.ToString(), user.Email, user.FullName);
        }
    }
}
