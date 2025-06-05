using ControleFluxoCaixa.Application.DTOs.Auth;
using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Auth.RefreshToken
{
    /// <summary>
    /// Comando para solicitação de novo token de acesso com base em um refresh token válido.
    /// </summary>
    public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshDto>;
}
