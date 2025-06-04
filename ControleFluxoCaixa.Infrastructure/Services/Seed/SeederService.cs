using ControleFluxoCaixa.Application.Interfaces.Seed;
using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Infrastructure.Context.Identity;
using Microsoft.EntityFrameworkCore;

namespace ControleFluxoCaixa.Infrastructure.Services.Seed
{
    /// <summary>
    /// Implementação do serviço de controle de execução de scripts de seed.
    /// Permite verificar e registrar quais scripts já foram executados no banco.
    /// </summary>
    public class SeederService : ISeederService
    {
        private readonly IdentityDBContext _context;

        /// <summary>
        /// Construtor que recebe o contexto do banco de dados de identidade.
        /// </summary>
        /// <param name="context">DbContext usado para acessar a tabela SeedHistory.</param>
        public SeederService(IdentityDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica se um seed com o nome informado já foi executado com sucesso.
        /// </summary>
        /// <param name="seedName">Nome único do seed (ex: "SeedUsuarioAdmin").</param>
        /// <returns>Retorna true se o seed já foi executado com sucesso.</returns>
        public async Task<bool> HasRunAsync(string seedName)
        {
            return await _context.SeedHistory
                .AnyAsync(s => s.SeedName == seedName && s.Succeeded);
        }

        /// <summary>
        /// Marca um seed como executado, registrando nome, sucesso e quem executou.
        /// </summary>
        /// <param name="seedName">Nome único do seed.</param>
        /// <param name="succeeded">True se a execução foi bem-sucedida.</param>
        /// <param name="executedBy">Identificador de quem executou (pode ser e-mail, "sistema", etc).</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task MarkAsRunAsync(string seedName, bool succeeded, string executedBy)
        {
            var record = new SeedHistory
            {
                Id = Guid.NewGuid(),
                SeedName = seedName,
                ExecutedAt = DateTime.UtcNow,
                ExecutedBy = executedBy,
                Succeeded = succeeded
            };

            _context.SeedHistory.Add(record);
            await _context.SaveChangesAsync();
        }
    }
}
