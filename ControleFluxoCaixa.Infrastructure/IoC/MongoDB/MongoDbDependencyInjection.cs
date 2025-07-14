using ControleFluxoCaixa.Mongo.Repositories;
using ControleFluxoCaixa.Mongo.Settings;
using ControleFluxoCaixa.MongoDB.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ControleFluxoCaixa.Infrastructure.IoC.MongoDB
{
    public static class MongoDbInjection
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            // Lê a seção "Mongo" do appsettings.json e vincula aos atributos de MongoDbSettings
            services.Configure<MongoDbSettings>(configuration.GetSection("Mongo"));

            // Registra o MongoClient como Singleton (recomendado oficialmente pelo driver)
            // O MongoClient é thread-safe e gerencia automaticamente o pool de conexões internas.
            // Criar mais de uma instância em uma aplicação é desperdício de recursos.
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            // Registra o IMongoDatabase como Scoped (uma instância por requisição HTTP)
            // Explicação:
            // - Embora IMongoDatabase seja leve, usamos AddScoped para alinhar com o ciclo de vida da API.
            // - Isso garante que cada requisição obtenha uma instância isolada do contexto do banco.
            // - O MongoClient é reaproveitado (Singleton), e o GetDatabase apenas aponta para o banco nomeado.
            services.AddScoped(sp =>
            {
                var mongoSettings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            // Registra o repositório como Scoped para que ele use a instância de IMongoDatabase da requisição.
            // Isso garante consistência do ciclo de vida e evita conflitos em cenários multi-threading.
            services.AddScoped<ISaldoDiarioConsolidadoRepository, SaldoDiarioConsolidadoRepository>();

            return services;
        }
    }
}
