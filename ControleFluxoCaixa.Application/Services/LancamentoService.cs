using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Application.Interfaces.Lancamentos;
using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Interfaces;

namespace ControleFluxoCaixa.Application.Services.Lancamentos
{
    /// <summary>
    /// Serviço de aplicação responsável por orquestrar regras de negócio de lançamentos,
    /// incluindo uso de cache, persistência e transformação de dados.
    /// </summary>
    public class LancamentoService : ILancamentoService
    {
        private readonly ILancamentoRepository _repository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;

        public LancamentoService(
            ILancamentoRepository repository,
            ICacheService cache,
            IMapper mapper)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém um lançamento por ID, utilizando o cache Redis para otimização.
        /// </summary>
        public async Task<ItenLancando?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var key = $"lancamento:{id}";

            return await _cache.GetOrSetAsync(key,
                async () =>
                {
                    var entity = await _repository.GetByIdAsync(id, cancellationToken);
                    return entity is null ? null : _mapper.Map<ItenLancando>(entity);
                },
                TimeSpan.FromMinutes(5),
                cancellationToken);
        }

        /// <summary>
        /// Obtém todos os lançamentos, com cache compartilhado para performance.
        /// </summary>
        public async Task<List<ItenLancando>> ObterTodosAsync(CancellationToken cancellationToken)
        {
            var key = "lancamentos:all";

            return await _cache.GetOrSetAsync(key,
                async () =>
                {
                    var entities = await _repository.GetAllAsync(cancellationToken);
                    return _mapper.Map<List<ItenLancando>>(entities);
                },
                TimeSpan.FromMinutes(5),
                cancellationToken);
        }

        /// <summary>
        /// Cria um ou mais lançamentos no banco e remove o cache global da listagem.
        /// </summary>
        public async Task<List<Guid>> CriarAsync(List<DTOs.Itens> dtos, CancellationToken cancellationToken)
        {
            var ids = new List<Guid>();

            foreach (var dto in dtos)
            {
                var entity = _mapper.Map<Lancamento>(dto);
                await _repository.CreateAsync(entity, cancellationToken);
                ids.Add(entity.Id);
            }

            // Invalida cache da listagem geral
            await _cache.RemoveAsync("lancamentos:all", cancellationToken);

            return ids;
        }

        /// <summary>
        /// Exclui um lançamento por ID e remove do cache individual e geral.
        /// </summary>
        public async Task<bool> ExcluirAsync(Guid id, CancellationToken cancellationToken)
        {
            var success = await _repository.DeleteAsync(id, cancellationToken);

            if (success)
            {
                await _cache.RemoveAsync("lancamentos:all", cancellationToken);
                await _cache.RemoveAsync($"lancamento:{id}", cancellationToken);
            }

            return success;
        }
    }
}
