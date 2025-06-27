using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFluxoCaixa.Infrastructure.Configurations
{
    public class LancamentoConfigurations : IEntityTypeConfiguration<ControleFluxoCaixa.Domain.Entities.Lancamento>
    {
        public void Configure(EntityTypeBuilder<ControleFluxoCaixa.Domain.Entities.Lancamento> builder)
        {
            builder.ToTable("Lancamentos");

            // Chave primária
            builder.HasKey(l => l.Id);

            // Id como CHAR(36) (GUID)
            builder.Property(l => l.Id)
                   .HasColumnType("char(36)")
                   .IsRequired();

            // Data do lançamento
            builder.Property(l => l.Data)
                   .HasColumnType("datetime(6)")
                   .IsRequired();

            // Valor
            builder.Property(l => l.Valor)
                   .HasPrecision(18, 2)
                   .IsRequired();

            // Tipo (enum int)
            builder.Property(l => l.Tipo)
                   .HasColumnType("int")
                   .IsRequired();

            // Descrição
            builder.Property(l => l.Descricao)
                   .HasMaxLength(100)
                   .HasColumnType("varchar(100)")
                   .IsRequired();

        }
    }
}
