using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Enums;

namespace ControleFluxoCaixa.Domain.Interfaces
{
    /// <summary>
    /// Interface para operações relacionadas aos lançamentos no repositório.
    /// </summary>
    public interface ILancamentoRepository
    {
        /// <summary>
        /// Cria um novo lançamento na base de dados.
        /// </summary>
        Task CreateAsync(Lancamento lancamento, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna um lançamento pelo ID.
        /// </summary>
        Task<Lancamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna todos os lançamentos.
        /// </summary>
        Task<List<Lancamento>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Exclui um lançamento pelo ID.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna todos os lançamentos de um determinado tipo.
        /// </summary>
        Task<List<Lancamento>> GetByTipoAsync(TipoLancamento tipo, CancellationToken cancellationToken);

    }
}
