using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Manipulador da requisição que lista todos os lançamentos do sistema.
    /// </summary>
    public class ListLancamentosQueryHandler : IRequestHandler<ListLancamentosQuery, List<Itens>>
    {
        private readonly ILancamentoRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ListLancamentosQueryHandler> _logger;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        /// <param name="repository">Repositório de lançamentos que acessa os dados.</param>
        /// <param name="mapper">AutoMapper para converter entidade em DTO.</param>
        /// <param name="logger">Logger para registrar informações da operação.</param>
        public ListLancamentosQueryHandler(
            ILancamentoRepository repository,
            IMapper mapper,
            ILogger<ListLancamentosQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Lida com a consulta que retorna todos os lançamentos do banco.
        /// </summary>
        /// <param name="request">Objeto da query (sem parâmetros neste caso).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de lançamentos convertida para o DTO `Itens`.</returns>
        public async Task<List<Itens>> Handle(ListLancamentosQuery request, CancellationToken cancellationToken)
        {
            // Recupera todos os lançamentos da base de dados.
            var lancamentos = await _repository.GetAllAsync(cancellationToken);

            // Registra no log a quantidade de lançamentos encontrados.
            _logger.LogInformation("[ListHandler] {Count} lançamentos recuperados.", lancamentos.Count);

            // Converte os lançamentos (entidades) para a lista de DTOs (Itens).
            return _mapper.Map<List<Itens>>(lancamentos);
        }
    }
}
