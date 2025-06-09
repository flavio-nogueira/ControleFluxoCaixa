using ControleFluxoCaixa.Domain.Enums;

namespace ControleFluxoCaixa.Application.DTOs
{
    /// <summary>
    /// DTO que representa um item de lançamento financeiro, usado em operações como criação ou exclusão.
    /// </summary>
    public class Itens
    {
        /// <summary>
        /// Data do lançamento.
        /// Representa quando o valor foi debitado ou creditado.
        /// </summary>
        public DateTime Data { get; set; }

        /// <summary>
        /// Valor monetário do lançamento.
        /// Pode ser positivo (crédito) ou negativo (débito), dependendo do tipo.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição do lançamento.
        /// Exemplo: "Pagamento de conta", "Recebimento de salário", etc.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do lançamento: Crédito ou Débito.
        /// Utiliza o enum TipoLancamento definido no domínio.
        /// </summary>
        public TipoLancamento Tipo { get; set; }
    }
}
