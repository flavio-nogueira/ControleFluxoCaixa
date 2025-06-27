using ControleFluxoCaixa.Application.DTOs.Response;
using MediatR;

namespace ControleFluxoCaixa.Application.Commands.Lancamento
{
    /// <summary>
    /// Comando para criar múltiplos lançamentos.
    /// </summary>
    public class CreateLancamentoCommand : IRequest<LancamentoResponseDto>
    {
        /// <summary>
        /// Lista de lançamentos a serem criados.
        /// </summary>
        public List<DTOs.Itens> Itens { get; set; } = new();
    }
}
