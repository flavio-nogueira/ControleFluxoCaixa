using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Auth.UpdateUser
{
    /// <summary>
    /// Comando para atualizar os dados de um usuário existente, incluindo senha.
    /// </summary>
    public record UpdateUserCommand(string Id, string? Email, string? FullName, string? NewPassword) : IRequest;
}
