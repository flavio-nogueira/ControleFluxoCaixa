using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Query para obter todos os usuários do sistema.
    /// </summary>
    public record GetAllUsersQuery : IRequest<List<UserDto>>;
}
