using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Domain.Enums;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Query para retornar todos os lançamentos de um tipo específico (Débito ou Crédito).
    /// Utiliza o padrão CQRS com MediatR.
    /// </summary>
    public class GetLancamentosByTipoQuery : IRequest<IEnumerable<ItenLancando>>
    {
        /// <summary>
        /// Tipo de lançamento a ser filtrado (Débito ou Crédito).
        /// </summary>
        public TipoLancamento Tipo { get; }

        /// <summary>
        /// Construtor da query que recebe o tipo desejado.
        /// </summary>
        /// <param name="tipo">Tipo de lançamento (Débito ou Crédito)</param>
        public GetLancamentosByTipoQuery(TipoLancamento tipo)
        {
            Tipo = tipo;
        }
    }
}
