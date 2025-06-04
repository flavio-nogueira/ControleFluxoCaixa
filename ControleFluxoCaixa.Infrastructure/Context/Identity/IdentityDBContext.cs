using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControleFluxoCaixa.Infrastructure.Context.Identity
{
    public class IdentityDBContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        /// <summary>
        /// Representa a tabela de RefreshTokens no banco de dados.
        /// Cada instância de RefreshToken armazena tokens de atualização vinculados a um usuário.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        /// <summary>
        /// Construtor que recebe as opções de configuração do DbContext (connection string, provedor, etc.).
        /// Essas opções são injetadas via DI no Startup ou Program.
        /// </summary>
        /// <param name="opts">Opções de configuração do IdentityContext.</param>
        public IdentityDBContext(DbContextOptions<IdentityDBContext> opts)
            : base(opts)
        {
            // A chamada a base(opts) já configura internamente o DbContext do Identity.
        }

        /// <summary>
        /// Tabela responsável por registrar histórico de execuções de seeds.
        /// </summary>
        public DbSet<SeedHistory> SeedHistory { get; set; } = default!;
        /// <summary>
        /// Configurações adicionais de mapeamento de entidades.
        /// Aqui definimos o relacionamento entre RefreshToken e ApplicationUser.
        /// </summary>
        /// <param name="builder">Construtor de modelo fornecido pelo EF Core.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Importante chamar o método base para que o Identity configure suas tabelas padrão.
            base.OnModelCreating(builder);

            // Configura o relacionamento:
            // - Um RefreshToken pertence a um único ApplicationUser (propriedade User).
            // - Um ApplicationUser pode ter vários RefreshTokens (WithMany sem navegação explícita).
            // - A chave estrangeira em RefreshToken é UserId.
            // - OnDelete Cascade: se o usuário for removido, todos os RefreshTokens relacionados também são removidos.
            builder.Entity<RefreshToken>()
                   .HasOne(rt => rt.User)        // RefreshToken.User é a entidade ApplicationUser
                   .WithMany()                   // ApplicationUser não possui coleção explícita de RefreshTokens
                   .HasForeignKey(rt => rt.UserId) // Chave estrangeira na tabela RefreshTokens
                   .OnDelete(DeleteBehavior.Cascade); // Cascateia exclusão do usuário aos seus tokens

            // Mapeamento da tabela SeedHistory
            builder.Entity<SeedHistory>(entity =>
            {
                entity.ToTable("SeedHistory");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.SeedName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasIndex(e => e.SeedName)
                      .IsUnique();

                entity.Property(e => e.ExecutedAt)
                      .IsRequired();

                entity.Property(e => e.ExecutedBy)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Succeeded)
                      .IsRequired();
            });
        }
    }
}
