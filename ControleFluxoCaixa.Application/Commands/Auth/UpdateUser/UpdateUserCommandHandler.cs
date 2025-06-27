using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Commands.Auth.UpdateUser
{
    /// <summary>
    /// Handler que realiza atualizações nos dados de um usuário, incluindo a redefinição de senha.
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        // Construtor com injeção de dependências: gerenciador de usuários, serviço de cache e logger
        public UpdateUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICacheService cache,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        // Manipulador do comando de atualização de usuário
        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado para atualização: {Id}", request.Id);
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            // Atualiza o e-mail e o UserName, se forem diferentes do atual
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                user.Email = request.Email;
                user.UserName = request.Email;
            }

            // Atualiza o nome completo, se fornecido
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName;
            }

            // Persiste as alterações do usuário no banco
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Erro ao atualizar dados do usuário {UserId}: {Erros}", user.Id, updateResult.Errors);
                throw new Exception("Erro ao atualizar dados do usuário.");
            }

            // Se foi enviada uma nova senha, realiza a redefinição
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                // Gera um token para redefinição de senha
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Aplica a nova senha
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
                if (!resetResult.Succeeded)
                {
                    _logger.LogError("Erro ao redefinir senha para o usuário {UserId}: {Erros}", user.Id, resetResult.Errors);
                    throw new Exception("Erro ao redefinir a senha.");
                }
            }

            // Remove o cache do usuário e da lista de usuários
            await _cache.RemoveAsync($"user:{user.Id}", cancellationToken);
            await _cache.RemoveAsync("users:all", cancellationToken);

            _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", user.Id);

            // Retorna um resultado de sucesso sem conteúdo
            return Unit.Value;
        }
    }
}
