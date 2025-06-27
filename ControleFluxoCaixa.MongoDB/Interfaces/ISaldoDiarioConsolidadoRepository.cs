using ControleFluxoCaixa.Mongo.Documents;

namespace ControleFluxoCaixa.MongoDB.Interfaces
{
    /// <summary>
    /// Contrato para atualizar e consultar o saldo diário consolidado no MongoDB.
    /// </summary>
    public interface ISaldoDiarioConsolidadoRepository
    {
        /// <summary>
        /// Atualiza (upsert) o documento de saldo para a data especificada:
        ///   - Se for tipo “entrada”, incrementa TotalEntradas e Saldo.
        ///   - Se for tipo “saída”, incrementa TotalSaidas e decrementa Saldo.
        /// </summary>
        Task UpdateAsync(DateTime data, decimal valor, string tipoFila, bool isEntrada, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna todos os saldos diários consolidados entre dataInicial e dataFinal (inclusive),
        /// ordenados por data ascendente.
        /// </summary>
        Task<List<SaldoDiarioConsolidado>> GetBetweenAsync(DateTime dataInicial, DateTime dataFinal, CancellationToken cancellationToken);
    }
}
