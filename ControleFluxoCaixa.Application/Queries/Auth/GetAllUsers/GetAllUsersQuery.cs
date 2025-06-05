
using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries.Auth.GetAllUsers
{
    /// <summary>
    /// Query para obter todos os usuários do sistema.
    /// </summary>
    public record GetAllUsersQuery : IRequest<List<UserDto>>;
}
