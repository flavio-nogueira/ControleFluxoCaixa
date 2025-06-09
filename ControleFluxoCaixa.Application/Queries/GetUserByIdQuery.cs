using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Query para obter os dados de um usuário por seu ID.
    /// </summary>
    public record GetUserByIdQuery(string Id) : IRequest<UserDto?>;
}