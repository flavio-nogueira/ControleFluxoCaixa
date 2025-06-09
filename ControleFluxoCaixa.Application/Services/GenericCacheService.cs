using ControleFluxoCaixa.Application.Interfaces.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Infrastructure.Cache
{
    /// <summary>
    /// Implementação genérica de cache utilizando IMemoryCache.
    /// Ideal para cenários onde o cache local em memória é suficiente.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        /// <summary>
        /// Construtor do serviço de cache, injetando dependências do sistema.
        /// </summary>
        /// <param name="memoryCache">Serviço de cache em memória fornecido pelo ASP.NET Core.</param>
        /// <param name="logger">Logger para registro de eventos e erros.</param>
        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Obtém um valor do cache ou cria e armazena usando a função factory se não existir.
        /// </summary>
        /// <typeparam name="T">Tipo do valor armazenado no cache.</typeparam>
        /// <param name="key">Chave única que identifica o valor no cache.</param>
        /// <param name="factory">Função que será executada para gerar o valor caso ele não exista.</param>
        /// <param name="duration">Tempo de expiração do cache (Time-To-Live).</param>
        /// <param name="cancellationToken">Token para cancelamento assíncrono (não usado aqui, mas mantido por compatibilidade).</param>
        /// <returns>O valor recuperado ou criado.</returns>
        public async Task<T?> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue<T>(key, out var cached))
            {
                _logger.LogDebug("Valor encontrado no cache para a chave: {Key}", key);
                return cached;
            }

            _logger.LogDebug("Valor não encontrado no cache. Executando factory para a chave: {Key}", key);
            var result = await factory();

            if (result != null)
            {
                _memoryCache.Set(key, result, duration);
                _logger.LogDebug("Valor armazenado no cache com duração de {Duration} para a chave: {Key}", duration, key);
            }
            else
            {
                _logger.LogWarning("Factory retornou nulo para a chave: {Key}. Nada foi armazenado no cache.", key);
            }

            return result;
        }

        /// <summary>
        /// Remove um item do cache pela chave especificada.
        /// </summary>
        /// <param name="key">Chave do item a ser removido.</param>
        /// <param name="cancellationToken">Token de cancelamento (não usado, mas mantido para compatibilidade).</param>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removendo item do cache com chave: {Key}", key);
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
