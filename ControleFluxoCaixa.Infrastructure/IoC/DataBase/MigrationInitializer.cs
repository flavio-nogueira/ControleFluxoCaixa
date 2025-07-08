using ControleFluxoCaixa.Infrastructure.Context.FluxoCaixa;
using ControleFluxoCaixa.Infrastructure.Context.Identity;
using ControleFluxoCaixa.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class MigrationInitializer
{
    /// <summary>
    /// Aplica migrations pendentes e executa o seed inicial de usuários.
    /// </summary>
    /// <param name="app">Instância da aplicação (IHost)</param>
    public static async Task ApplyMigrationsAsync(IHost app)
    {
        // Cria um escopo de serviços para acessar os contextos e dependências
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Cria o logger para registrar o progresso e erros
        var logger = services.GetRequiredService<ILoggerFactory>()
                             .CreateLogger("MigrationInitializer");

        // MIGRATIONS: Banco Identity 
        try
        {
            var identityDb = services.GetRequiredService<IdentityDBContext>();
            var identityPending = await identityDb.Database.GetPendingMigrationsAsync();

            if (identityPending.Any())
            {
                logger.LogInformation("Aplicando migrations para banco Identity...");
                await identityDb.Database.MigrateAsync();
                logger.LogInformation("Migrations de Identity aplicadas com sucesso.");
            }
            else
            {
                logger.LogInformation("Nenhuma migration pendente para banco Identity.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrations de Identity.");
        }

        // MIGRATIONS: Banco Fluxo de Caixa ===
        try
        {
            var fluxoDb = services.GetRequiredService<FluxoCaixaDbContext>();
            var fluxoPending = await fluxoDb.Database.GetPendingMigrationsAsync();

            if (fluxoPending.Any())
            {
                logger.LogInformation("Aplicando migrations para banco Fluxo de Caixa...");
                await fluxoDb.Database.MigrateAsync();
                logger.LogInformation("Migrations de Fluxo de Caixa aplicadas com sucesso.");
            }
            else
            {
                logger.LogInformation("Nenhuma migration pendente para banco Fluxo de Caixa.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrations de Fluxo de Caixa.");
        }

        // SEED: Usuário Administrador ===
        try
        {
            var seeder = services.GetRequiredService<SeedIdentityAdminUser>();
            await seeder.ExecuteAsync();
            logger.LogInformation("Seed do usuário administrador executado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao executar seed do usuário administrador.");
        }
    }
}
