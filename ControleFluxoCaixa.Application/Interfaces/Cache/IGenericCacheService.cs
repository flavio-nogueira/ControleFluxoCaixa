namespace ControleFluxoCaixa.Application.Interfaces.Cache
{
    /// <summary>
    /// Interface genérica para serviços de cache distribuído ou em memória.
    /// Permite obter valores armazenados no cache ou criá-los dinamicamente caso ainda não existam,
    /// além de possibilitar a remoção manual de itens armazenados.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Tenta obter o valor em cache pela chave fornecida.
        /// Se não existir, executa a função de fábrica (factory), armazena o resultado no cache pelo tempo definido (duration)
        /// e retorna o valor.
        /// </summary>
        /// <typeparam name="T">Tipo do valor a ser armazenado ou recuperado do cache.</typeparam>
        /// <param name="key">Chave única utilizada para armazenar e buscar o item no cache.</param>
        /// <param name="factory">Função que gera o valor caso ele ainda não exista no cache.</param>
        /// <param name="duration">Tempo de expiração do item no cache (Time-To-Live).</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>O valor obtido do cache ou gerado pela factory.</returns>
        Task<T?> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan duration,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Remove manualmente um item do cache com base na chave fornecida.
        /// Útil para invalidação de dados quando há alterações em recursos relacionados.
        /// </summary>
        /// <param name="key">Chave do item a ser removido do cache.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default
        );
    }
}
