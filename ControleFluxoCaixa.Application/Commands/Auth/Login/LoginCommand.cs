using ControleFluxoCaixa.Application.DTOs.Auth;
using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Auth.Login
{
    /// <summary>
    /// Comando que representa a requisição de login com e-mail, senha e IP de origem.
    /// </summary>
    public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<RefreshDto>;
}