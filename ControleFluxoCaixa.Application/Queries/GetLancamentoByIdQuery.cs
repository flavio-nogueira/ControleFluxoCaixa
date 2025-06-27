using ControleFluxoCaixa.Application.DTOs;
using MediatR;

namespace ControleFluxoCaixa.Application.Queries
{
    /// <summary>
    /// Query para recuperar um lançamento específico pelo seu identificador único (ID).
    /// Utiliza o padrão CQRS com MediatR.
    /// </summary>
    public class GetLancamentoByIdQuery : IRequest<ItenLancando?>
    {
        /// <summary>
        /// Identificador único do lançamento a ser buscado.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Construtor que recebe o ID do lançamento desejado.
        /// </summary>
        /// <param name="id">GUID do lançamento</param>
        public GetLancamentoByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
