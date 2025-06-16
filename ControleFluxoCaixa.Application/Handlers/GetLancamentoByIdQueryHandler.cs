using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Handler responsável por tratar a consulta de um lançamento específico pelo seu ID.
    /// </summary>
    public class GetLancamentoByIdQueryHandler
        : IRequestHandler<GetLancamentoByIdQuery, ItenLancando?>
    {
        private readonly ILancamentoRepository _repository;
        private readonly ILogger<GetLancamentoByIdQueryHandler> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        /// <param name="repository">Repositório de acesso aos lançamentos.</param>
        /// <param name="logger">Logger para registro de logs.</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e DTOs.</param>
        public GetLancamentoByIdQueryHandler(
            ILancamentoRepository repository,
            ILogger<GetLancamentoByIdQueryHandler> logger,
            IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Manipula a requisição para buscar um lançamento pelo seu ID.
        /// </summary>
        /// <param name="request">Objeto contendo o ID do lançamento.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação.</param>
        /// <returns>DTO do lançamento encontrado ou null se não existir.</returns>
        public async Task<ItenLancando?> Handle(GetLancamentoByIdQuery request, CancellationToken cancellationToken)
        {
            // Registra a tentativa de busca pelo ID informado
            _logger.LogInformation("Buscando lançamento por ID: {Id}", request.Id);

            // Consulta o repositório
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            // Se não encontrado, retorna null
            if (entity == null)
                return null;

            // Mapeia a entidade para DTO e retorna
            return _mapper.Map<ItenLancando>(entity);
        }
    }
}
