using ControleFluxoCaixa.Infrastructure.Context.Identity;
using ControleFluxoCaixa.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ControleFluxoCaixa.Infrastructure.IoC.DataBase
{
    /// <summary>
    /// Classe responsável por aplicar automaticamente as migrations pendentes
    /// do IdentityDBContext assim que a aplicação inicia.
    /// Além disso, executa os scripts de seed necessários para criar dados iniciais,
    /// como o usuário administrador padrão.
    /// </summary>
    public static class MigrationInitializer
    {
        /// <summary>
        /// Aplica as migrations pendentes do banco de dados de identidade e executa seeds.
        /// Deve ser chamada logo após a construção do host da aplicação.
        /// </summary>
        /// <param name="app">Instância do IHost gerado pelo WebApplication</param>
        public static async Task ApplyMigrationsAsync(IHost app)
        {
            // Cria um escopo de serviço para acessar o container de injeção de dependência (DI)
            using var scope = app.Services.CreateScope();

            // Obtém uma instância do IdentityDBContext dentro do escopo
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDBContext>();

            // Verifica se existem migrations que ainda não foram aplicadas no banco de dados de forma assíncrona, a lista de migrations pendentes
            var pending = await dbContext.Database.GetPendingMigrationsAsync();

            if (pending.Any())
            {
                // Se houver migrations pendentes, aplica todas automaticamente
                Console.WriteLine("Aplicando migrations pendentes...");
                await dbContext.Database.MigrateAsync(); // Aplica migrations de forma assíncrona
            }
            else
            {
                // Se não houver nenhuma migration pendente, apenas informa
                Console.WriteLine("Nenhuma migration pendente.");
            }

            try
            {
                // Resolve o serviço de seed para criar o usuário admin
                var seeder = scope.ServiceProvider.GetRequiredService<SeedIdentityAdminUser>();

                // Executa o seed (ex: cria o usuário Admin se necessário)
                await seeder.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Erro ao executar o seed:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
    }
}
