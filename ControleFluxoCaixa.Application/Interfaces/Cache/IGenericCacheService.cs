namespace ControleFluxoCaixa.Application.Interfaces.Cache
{
    public interface IGenericCacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan duration, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }

}
