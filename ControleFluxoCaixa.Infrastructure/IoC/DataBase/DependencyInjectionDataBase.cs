using ControleFluxoCaixa.Infrastructure.Context.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFluxoCaixa.Infrastructure.IoC.DataBase
{
    // Torne esta classe estática
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
            // 2) Configuração do DbContext para Identity (autenticação/usuários)    
            // Recupera a connection string "IdentityConnection" no appsettings.json.
            var identityConn = configuration.GetConnectionString("IdentityConnection")
                             ?? throw new InvalidOperationException("Connection string 'IdentityConnection' não encontrada.");
            // Registra o IdentityContext usando MySQL como provedor de dados do ASP.NET Identity.
            services.AddDbContext<IdentityDBContext>(options =>
                options.UseMySql(identityConn, ServerVersion.AutoDetect(identityConn)));

            return services;
        }
    }
}
