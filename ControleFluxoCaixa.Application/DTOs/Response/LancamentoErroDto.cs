using ControleFluxoCaixa.Application.Converters;
using ControleFluxoCaixa.Domain.Enums;
using System.Text.Json.Serialization;

namespace ControleFluxoCaixa.Application.DTOs.Response
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
        public Guid? Id { get; set; } = null;

        /// <summary>
        /// Data do lançamento que falhou. O formato é convertido usando um conversor customizado para JSON.
        /// </summary>
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime Data { get; set; }

        /// <summary>
        /// Valor financeiro do lançamento que apresentou erro.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição informada para o lançamento.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do lançamento (Débito ou Crédito), conforme definido no enum TipoLancamento.
        /// </summary>
        public TipoLancamento Tipo { get; set; }

        /// <summary>
        /// Mensagem de erro explicando o motivo da falha no processamento do lançamento.
        /// </summary>
        public string Erro { get; set; } = string.Empty;
    }
}
