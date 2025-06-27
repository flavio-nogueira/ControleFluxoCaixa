using ControleFluxoCaixa.Gatware.BFF.Converters;
using ControleFluxoCaixa.Gatware.BFF.Dtos.Lancamento;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ControleFluxoCaixa.BFF.Dtos.Lancamento
{
    /// <summary>
    /// Representa o resultado de erro ao processar um item de lançamento em lote (batch).
    /// Essa classe é usada para detalhar os erros ocorridos durante a inclusão ou exclusão de lançamentos.
    /// </summary>
    public class LancamentoErroDto
    {
        /// <summary>
        /// Identificador único do lançamento. Pode ser nulo em caso de erro antes da criação de um ID.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Data do lançamento que falhou. Convertida com o conversor customizado (se necessário).
        /// </summary>
        [Required(ErrorMessage = "A data do lançamento é obrigatória.")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime Data { get; set; }

        /// <summary>
        /// Valor financeiro do lançamento que apresentou erro.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição informada para o lançamento.
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do lançamento (Débito ou Crédito).
        /// </summary>
        [Required(ErrorMessage = "O tipo de lançamento é obrigatório.")]
        public TipoLancamento Tipo { get; set; }

        /// <summary>
        /// Mensagem de erro explicando o motivo da falha no processamento.
        /// </summary>
        [Required(ErrorMessage = "A mensagem de erro é obrigatória.")]
        public string Erro { get; set; } = string.Empty;
    }
}
