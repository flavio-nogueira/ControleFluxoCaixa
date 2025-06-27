using ControleFluxoCaixa.Application.DTOs.Response;
using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Lancamento
{
    /// <summary>
    /// Comando para deletar um lançamento existente pelo ID.
    /// </summary>
    public class DeleteLancamentoCommand : IRequest<LancamentoResponseDto>
    {
        /// <summary>
        /// Identificador do lançamento a ser excluído.
        /// </summary>
        public List<Guid> Ids { get; set; } = new();
    }
}
