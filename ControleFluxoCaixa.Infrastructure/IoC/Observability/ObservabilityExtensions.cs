using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ControleFluxoCaixa.Infrastructure.IoC.Observability
{
    /// <summary>
    /// Classe de extensão responsável por registrar os serviços de observabilidade da aplicação.
    /// Isso inclui health checks, telemetria, métricas e futuros serviços relacionados à resiliência e monitoramento.
    /// </summary>
    public static class ObservabilityExtensions
    {
        /// <summary>
        /// Registra os serviços de observabilidade no container de injeção de dependência.
        /// Inclui o HealthCheck para o banco de dados MySQL utilizado pelo módulo de autenticação (Identity).
        /// </summary>
        /// <param name="services">A coleção de serviços do ASP.NET Core.</param>
        /// <param name="configuration">A configuração da aplicação (geralmente proveniente do appsettings.json).</param>
        /// <returns>Retorna a própria IServiceCollection, permitindo encadeamento fluente de chamadas.</returns>
        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            // Recupera a connection string do banco de identidade (IdentityConnection)
            var identityConn = configuration.GetConnectionString("IdentityConnection")
                               ?? throw new InvalidOperationException("Connection string 'IdentityConnection' não encontrada.");

            // Registra o serviço de verificação de integridade (HealthCheck) para o MySQL.
            // A tag "ready" permite diferenciar esse check como parte do readiness probe.
            services.AddHealthChecks()
                .AddMySql(
                    connectionString: identityConn,
                    name: "mysql_identity",
                    tags: new[] { "ready" } // Usado em MapHealthChecks para /health/ready
                );

            // Futuramente, outros health checks (Redis, RabbitMQ, etc) podem ser adicionados aqui.

            return services;
        }
    }
}
