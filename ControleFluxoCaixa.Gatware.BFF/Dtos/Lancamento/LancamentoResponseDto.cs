using ControleFluxoCaixa.BFF.Dtos.Lancamento;
using System.ComponentModel.DataAnnotations;

namespace ControleFluxoCaixa.BFF.Dtos.Lancamentos
{
    /// <summary>
    /// DTO utilizado para retornar o resultado de uma operação de lançamento (criação, exclusão, etc).
    /// Pode conter informações de sucesso, falha e erros detalhados por item.
    /// </summary>
    public class LancamentoResponseDto
    {
        /// <summary>
        /// Mensagem geral da operação, como "Todos os lançamentos foram criados com sucesso" ou "Alguns lançamentos falharam".
        /// </summary>
        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        public string Mensagem { get; set; } = string.Empty;

        /// <summary>
        /// Indica se a operação foi bem-sucedida (true) ou se houve falhas (false).
        /// </summary>
        [Required]
        public bool Sucesso { get; set; }

        /// <summary>
        /// Quantidade total de registros processados (incluindo os que deram certo ou falharam).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade de registros deve ser maior ou igual a zero.")]
        public int Registros { get; set; }

        /// <summary>
        /// Lista genérica contendo os retornos com sucesso (ex: IDs de lançamentos criados ou excluídos).
        /// Tipado como object para permitir flexibilidade (pode conter Guid, DTOs, etc).
        /// </summary>
        [Required]
        public List<object> Retorno { get; set; } = new();

        /// <summary>
        /// Lista contendo os detalhes dos erros que ocorreram em cada item durante o processamento.
        /// Cada erro possui informações como ID, data, valor e mensagem de erro.
        /// </summary>
        public List<LancamentoErroDto> Erros { get; set; } = new();
    }
}
