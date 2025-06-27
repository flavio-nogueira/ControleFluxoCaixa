using ControleFluxoCaixa.Application.Converters;
using ControleFluxoCaixa.Domain.Enums;
using System.Text.Json.Serialization;

namespace ControleFluxoCaixa.Application.DTOs
{
    public class ItenLancando
    {
        /// <summary>
        /// Identificador único do lançamento (GUID).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data em que o lançamento foi efetuado.
        /// </summary>
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime Data { get; set; }

        /// <summary>
        /// Valor monetário do lançamento.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição livre para detalhar o lançamento.
        /// Inicializa como string vazia para evitar nulls.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do lançamento (Débito ou Crédito), definido pelo enum TipoLancamento.
        /// </summary>
        public TipoLancamento Tipo { get; set; }
    }

}
