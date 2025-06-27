using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Query para obter todos os lançamentos.
    /// </summary>
    public class GetAllLancamentosQuery : IRequest<IEnumerable<ItenLancando>>
    {
    }
}
