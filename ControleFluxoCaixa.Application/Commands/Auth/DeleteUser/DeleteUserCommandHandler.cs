// Importa as entidades de domínio relacionadas ao usuário
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Application.Commands.Auth.Login;
using ControleFluxoCaixa.Application.Commands.Auth.RefreshToken;
using ControleFluxoCaixa.Application.Commands.Auth.RegisterUser;
using ControleFluxoCaixa.Application.Commands.Auth.UpdateUser;
using ControleFluxoCaixa.Application.Commands.Auth.DeleteUser;
using ControleFluxoCaixa.Application.Queries.Auth.GetAllUsers;
using ControleFluxoCaixa.Application.Queries.Auth.GetUserById;
using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using MediatR;

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
