using ControleFluxoCaixa.Gatware.BFF.Dtos.Lancamento;
using System;
using System.ComponentModel.DataAnnotations;


namespace ControleFluxoCaixa.BFF.Dtos.Lancamento
{
    /// <summary>
    /// DTO que representa um item de lançamento financeiro, usado em operações como criação ou exclusão.
    /// </summary>
    public class LancamentoDto 
    {
        /// <summary>
        /// Data do lançamento. Representa quando o valor foi debitado ou creditado.
        /// </summary>
        [Required(ErrorMessage = "A data do lançamento é obrigatória.")]
        public DateTime Data { get; set; }

        /// <summary>
        /// Valor monetário do lançamento.
        /// Pode ser positivo (crédito) ou negativo (débito), dependendo do tipo.
        /// </summary>
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição do lançamento.
        /// Exemplo: "Pagamento de conta", "Recebimento de salário", etc.
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do lançamento: Crédito ou Débito.
        /// </summary>
        [Required(ErrorMessage = "O tipo do lançamento é obrigatório.")]
        public TipoLancamento Tipo { get; set; }
    }
}
