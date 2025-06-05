using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Auth.RegisterUser
{
    /// <summary>
    /// Comando para registrar um novo usuário no sistema.
    /// </summary>
    public record RegisterUserCommand(string Email, string Password, string FullName) : IRequest<UserDto>;
}
