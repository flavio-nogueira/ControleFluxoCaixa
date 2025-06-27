using ControleFluxoCaixa.Infrastructure.Context.FluxoCaixa;
using ControleFluxoCaixa.Infrastructure.Context.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFluxoCaixa.Infrastructure.IoC.DataBase
{
    /// <summary>
    /// Classe estática responsável por configurar e registrar os DbContexts da aplicação.
    /// </summary>
    public static class DependencyInjectionDataBase
    {
        /// <summary>
        /// Método de extensão que registra todos os serviços de infraestrutura
        /// necessários para o domínio de ControleFluxoCaixa.
        /// </summary>
        /// <param name="services">Coleção de serviços do ASP.NET Core.</param>
        /// <param name="configuration">Configuração da aplicação (appsettings.json, variáveis de ambiente, etc.).</param>
        /// <returns>IServiceCollection com todos os serviços registrados.</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //Configuração do DbContext para Identity (autenticação/usuários)
            var identityConn = configuration.GetConnectionString("IdentityConnection")
                               ?? throw new InvalidOperationException("Connection string 'IdentityConnection' não encontrada.");
            services.AddDbContext<IdentityDBContext>(options =>
                options.UseMySql(identityConn, ServerVersion.AutoDetect(identityConn)));

            //Configuração do DbContext para Fluxo de Caixa 
            var fluxoConn = configuration.GetConnectionString("FluxoCaixaConnection")
                            ?? throw new InvalidOperationException("Connection string 'FluxoCaixaConnection' não encontrada.");
            services.AddDbContext<FluxoCaixaDbContext>(options =>
                options.UseMySql(fluxoConn, ServerVersion.AutoDetect(fluxoConn)));

            return services;
        }
    }
}
