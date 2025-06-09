using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Consulta para listar todos os lançamentos.
    /// </summary>
    public class ListLancamentosQuery : IRequest<List<Itens>>
    {
    }
}
