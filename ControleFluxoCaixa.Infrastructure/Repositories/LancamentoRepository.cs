using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Domain.Interfaces;
using ControleFluxoCaixa.Infrastructure.Context.FluxoCaixa;
using Microsoft.EntityFrameworkCore;

namespace ControleFluxoCaixa.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação concreta de ILancamentoRepository usando Entity Framework Core.
    /// Responsável pelo acesso direto à base de dados.
    /// </summary>
    public class LancamentoRepository : ILancamentoRepository
    {
        private readonly FluxoCaixaDbContext _context;

        public LancamentoRepository(FluxoCaixaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cria e persiste um novo lançamento.
        /// </summary>
        public async Task CreateAsync(Lancamento lancamento, CancellationToken cancellationToken)
        {
            await _context.Set<Lancamento>().AddAsync(lancamento, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Busca um lançamento pelo ID.
        /// </summary>  
        public async Task<Lancamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<Lancamento>()
                                 .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Retorna todos os lançamentos.
        /// </summary>
        public async Task<List<Lancamento>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<Lancamento>().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Remove um lançamento e salva alterações.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            _context.Set<Lancamento>().Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        ///  Retorna lançamentos pelo tipo.
        /// </summary>
        public async Task<List<Lancamento>> GetByTipoAsync(TipoLancamento tipo, CancellationToken cancellationToken)
        {
            return await _context.Set<Lancamento>()
                                 .Where(l => l.Tipo == tipo)
                                 .ToListAsync(cancellationToken);
        }

    }
}
