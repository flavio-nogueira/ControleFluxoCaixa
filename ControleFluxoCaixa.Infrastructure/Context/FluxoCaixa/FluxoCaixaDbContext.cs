using ControleFluxoCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFluxoCaixa.Infrastructure.Context.FluxoCaixa
{
    /// <summary>
    /// Contexto do Entity Framework para acessar as tabelas do banco de dados.
    /// </summary>
    public class FluxoCaixaDbContext : DbContext
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração do DbContext (connection string, provedor, etc.).
        /// </summary>
        /// <param name="options">Opções de configuração do contexto (injetadas via DI).</param>
        public FluxoCaixaDbContext(DbContextOptions<FluxoCaixaDbContext> options)
            : base(options)
        {
            // A chamada a base(options) já configura internamente o DbContext.
        }

        // 1) Declarar o DbSet para expor a entidade Lancamento como uma tabela no banco
        /// <summary>
        /// Representa a coleção de lançamentos no banco de dados.
        /// Cada instância de <see cref="Lancamento"/> será mapeada para uma linha na tabela correspondente.
        /// </summary>
        public DbSet<Lancamento> Lancamentos { get; set; } = null!;

        /// <summary>
        /// Método chamado pelo EF Core ao criar o modelo de dados.
        /// Aqui podemos aplicar configurações de mapeamento (tabelas, colunas, chaves, índices, relacionamentos, etc.).
        /// </summary>
        /// <param name="modelBuilder">Construtor de modelo fornecido pelo EF Core.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //   Se houver classes que implementam IEntityTypeConfiguration<T> neste assembly,
            //    o EF irá aplicar todas automaticamente via ApplyConfigurationsFromAssembly.
            //    Por exemplo: LancamentoMap : IEntityTypeConfiguration<Lancamento>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FluxoCaixaDbContext).Assembly);

            // Chamada ao método base para garantir configurações padrão do EF Core
            base.OnModelCreating(modelBuilder);
        }
    }
}


//dotnet ef migrations add InitialCreate_FluxoCaixa --project .\ControleFluxoCaixa.Infrastructure\ControleFluxoCaixa.Infrastructure.csproj --startup-project .\ControleFluxoCaixa.API\ControleFluxoCaixa.API.csproj --context FluxoCaixaDbContext
//dotnet ef database update --project .\ControleFluxoCaixa.Infrastructure\ControleFluxoCaixa.Infrastructure.csproj --startup-project .\ControleFluxoCaixa.API\ControleFluxoCaixa.API.csproj --context FluxoCaixaDbContext

