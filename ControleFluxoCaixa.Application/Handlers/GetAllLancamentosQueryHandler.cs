using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar a requisição de busca de todos os lançamentos cadastrados.
    /// </summary>
    public class GetAllLancamentosQueryHandler
        : IRequestHandler<GetAllLancamentosQuery, IEnumerable<ItenLancando>>
    {
        private readonly ILancamentoRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllLancamentosQueryHandler> _logger;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        /// <param name="repository">Repositório responsável pelo acesso aos dados dos lançamentos.</param>
        /// <param name="mapper">AutoMapper para conversão de entidades em DTOs.</param>
        /// <param name="logger">Logger para registrar informações de execução.</param>
        public GetAllLancamentosQueryHandler(
            ILancamentoRepository repository,
            IMapper mapper,
            ILogger<GetAllLancamentosQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Manipula a execução da query para obter todos os lançamentos.
        /// </summary>
        /// <param name="request">Query sem parâmetros adicionais.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação.</param>
        /// <returns>Lista de lançamentos mapeados para o DTO ItenLancando.</returns>
        public async Task<IEnumerable<ItenLancando>> Handle(GetAllLancamentosQuery request, CancellationToken cancellationToken)
        {
            // Log de início da operação
            _logger.LogInformation("Executando GetAllLancamentosQuery...");

            // Busca os lançamentos no repositório
            var lancamentos = await _repository.GetAllAsync(cancellationToken);

            // Mapeia a lista de entidades para DTOs e retorna
            return _mapper.Map<IEnumerable<ItenLancando>>(lancamentos);
        }
    }
}
