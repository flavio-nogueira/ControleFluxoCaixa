// Importa as entidades de domínio relacionadas ao usuário
using ControleFluxoCaixa.Domain.Entities.User;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ControleFluxoCaixa.Application.Commands.Auth.DeleteUser
{
    /// <summary>
    /// Manipulador do comando DeleteUserCommand.
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Construtor do manipulador DeleteUserCommandHandler.
        /// </summary>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Lida com a remoção de um usuário.
        /// </summary>
        /// <param name="request">Comando contendo o ID do usuário.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Tarefa concluída.</returns>
        /// <exception cref="InvalidOperationException">Se o usuário não for encontrado ou ocorrer falha ao deletar.</exception>
        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Erro ao excluir o usuário: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return Unit.Value;
        }
    }
}
