using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace ControleFluxoCaixa.Infrastructure.IoC.Observability
{
    /// <summary>
    /// Classe de extensão responsável por registrar os serviços de observabilidade da aplicação.
    /// Isso inclui health checks para banco de dados, cache e mensageria.
    /// </summary>
    public static class ObservabilityExtensions
    {
        /// <summary>
        /// Registra os serviços de observabilidade (health checks) no container de DI.
        /// </summary>
        /// <param name="services">Coleção de serviços do ASP.NET Core.</param>
        /// <param name="configuration">Configuração da aplicação (ex: appsettings.json).</param>
        /// <returns>IServiceCollection com os health checks adicionados.</returns>
        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            // Recupera todas as connection strings do appsettings
            var identityConn = configuration.GetConnectionString("IdentityConnection")
                ?? throw new InvalidOperationException("Connection string 'IdentityConnection' não encontrada.");

            var caixaFluxoConn = configuration.GetConnectionString("FluxoCaixaConnection")
                ?? throw new InvalidOperationException("Connection string 'FluxoCaixaConnection' não encontrada.");

            //var redisConn = configuration.GetConnectionString("RedisConnection")
            //    ?? throw new InvalidOperationException("Connection string 'RedisConnection' não encontrada.");


            // RabbitMQ Inclusão
            var rabbitMqInclusaoConn = configuration["RabbitMqSettings:Inclusao:AmqpUri"]
                ?? throw new InvalidOperationException("Configuração 'RabbitMqSettings:Inclusao:AmqpUri' não encontrada.");

            // RabbitMQ Exclusão
            var rabbitMqExclusaoConn = configuration["RabbitMqSettings:Exclusao:AmqpUri"]
                ?? throw new InvalidOperationException("Configuração 'RabbitMqSettings:Exclusao:AmqpUri' não encontrada.");

            // Adiciona todos os health checks configurados
            services.AddHealthChecks()

                // MySQL - Identity
                .AddMySql(
                    identityConn,
                    name: "mysql_identity",
                    tags: new[] { "ready", "db", "identity" }
                )

                // MySQL - Fluxo de Caixa
                .AddMySql(
                    caixaFluxoConn,
                    name: "mysql_fluxo",
                    tags: new[] { "ready", "db", "fluxo" }
                )

                // RabbitMQ - Broker de Mensageria
                .AddRabbitMQ(
                    rabbitConnectionString: rabbitMqInclusaoConn,
                    name: "rabbitmq_inclusao",
                    tags: new[] { "ready", "queue", "rabbitmq", "inclusao" }
                )
                .AddRabbitMQ(
                    rabbitConnectionString: rabbitMqExclusaoConn,
                    name: "rabbitmq_exclusao",
                    tags: new[] { "ready", "queue", "rabbitmq", "exclusao" }
                );


            return services;
        }
    }
}
