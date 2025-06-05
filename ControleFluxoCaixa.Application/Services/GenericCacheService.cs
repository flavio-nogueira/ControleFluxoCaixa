using ControleFluxoCaixa.Application.Interfaces.Cache;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ControleFluxoCaixa.Application.Services
{
    /// <summary>
    /// Serviço genérico de cache que utiliza IDistributedCache (como Redis) 
    /// para armazenar qualquer tipo de objeto, com suporte a leitura, gravação e invalidação.
    /// Ideal para cenários com padrão CQRS ou para reduzir consultas repetitivas a repositórios.
    /// </summary>
    public class GenericCacheService : IGenericCacheService
    {
        // Dependência principal: IDistributedCache (pode ser Redis, SQL Server, etc.)
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Construtor que injeta a instância de cache distribuído.
        /// </summary>
        /// <param name="cache">Instância de IDistributedCache configurada no DI.</param>
        public GenericCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Tenta obter um valor do cache. Se não existir, executa a função fornecida (factory),
        /// armazena o resultado no cache com o tempo de expiração indicado e retorna o valor.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser armazenado e retornado.</typeparam>
        /// <param name="key">Chave única usada para identificar o item no cache.</param>
        /// <param name="factory">Função assíncrona que gera o valor caso o cache esteja vazio.</param>
        /// <param name="duration">Duração da validade do cache (expiração absoluta).</param>
        /// <param name="cancellationToken">Token opcional para cancelamento.</param>
        /// <returns>Instância do objeto armazenado, vinda do cache ou da função factory.</returns>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            // Tenta obter o valor do cache com base na chave informada
            var cached = await _cache.GetStringAsync(key, cancellationToken);

            // Se encontrou algo, desserializa do JSON para o tipo T e retorna
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<T>(cached);
            }

            // Se não encontrou, executa a função (factory) para obter o valor real
            var result = await factory();

            // Se o resultado não for nulo, serializa para JSON e salva no cache
            if (result != null)
            {
                var json = JsonSerializer.Serialize(result);

                // Define a expiração absoluta com base na duração passada como parâmetro
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = duration
                };

                // Armazena no cache com a chave, valor serializado e opções de expiração
                await _cache.SetStringAsync(key, json, options, cancellationToken);
            }

            // Retorna o resultado da factory (mesmo que nulo)
            return result;
        }

        /// <summary>
        /// Remove um item do cache com base na chave fornecida.
        /// Útil para invalidação manual após updates/deletes em bancos de dados.
        /// </summary>
        /// <param name="key">Chave do item que deve ser removido do cache.</param>
        /// <param name="cancellationToken">Token opcional para cancelamento.</param>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
