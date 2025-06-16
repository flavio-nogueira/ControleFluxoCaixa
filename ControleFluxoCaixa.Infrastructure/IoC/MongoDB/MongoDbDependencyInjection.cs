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
            services.Configure<MongoDbSettings>(configuration.GetSection("Mongo"));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddScoped(sp =>
            {
                var mongoSettings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            services.AddScoped<ISaldoDiarioConsolidadoRepository, SaldoDiarioConsolidadoRepository>();

            return services;
        }
    }
}
