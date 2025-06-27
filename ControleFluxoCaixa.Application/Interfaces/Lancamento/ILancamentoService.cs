using ControleFluxoCaixa.Application.DTOs;

namespace ControleFluxoCaixa.Application.Interfaces.Lancamentos
{
    /// <summary>
    /// Interface do serviço de aplicação responsável pelas regras de negócio
    /// relacionadas à entidade Lançamento.
    /// </summary>
    public interface ILancamentoService
    {
        /// <summary>
        /// Obtém os dados de um lançamento específico pelo seu ID.
        /// Pode utilizar cache para melhorar a performance.
        /// </summary>
        /// <param name="id">Identificador único do lançamento.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>DTO do lançamento, ou null se não encontrado.</returns>
        Task<ItenLancando?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna uma lista com todos os lançamentos cadastrados.
        /// Pode utilizar cache compartilhado para otimizar o acesso.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de DTOs representando os lançamentos.</returns>
        Task<List<ItenLancando>> ObterTodosAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Cria um ou mais lançamentos no banco de dados.
        /// O cache da listagem geral é invalidado após a criação.
        /// </summary>
        /// <param name="dtos">Lista de lançamentos a serem criados.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de IDs gerados para os lançamentos criados.</returns>
        Task<List<Guid>> CriarAsync(List<Itens> dtos, CancellationToken cancellationToken);

        /// <summary>
        /// Remove um lançamento específico do sistema.
        /// Remove também entradas de cache relacionadas.
        /// </summary>
        /// <param name="id">Identificador do lançamento a ser excluído.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>True se a exclusão foi realizada com sucesso; false caso contrário.</returns>
        Task<bool> ExcluirAsync(Guid id, CancellationToken cancellationToken);
    }
}
