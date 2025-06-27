using AutoMapper;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Interfaces;
using MediatR;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar a consulta de lançamentos filtrando por tipo (Crédito ou Débito).
    /// </summary>
    public class GetLancamentosByTipoQueryHandler
        : IRequestHandler<GetLancamentosByTipoQuery, IEnumerable<ItenLancando>>
    {
        private readonly ILancamentoRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Construtor que injeta dependências necessárias.
        /// </summary>
        /// <param name="repository">Repositório responsável pelas operações de dados.</param>
        /// <param name="mapper">AutoMapper para conversão de entidade para DTO.</param>
        public GetLancamentosByTipoQueryHandler(ILancamentoRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Manipula a requisição para retornar todos os lançamentos com o tipo especificado (Débito ou Crédito).
        /// </summary>
        /// <param name="request">Query contendo o tipo a ser filtrado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de lançamentos convertida para DTOs.</returns>
        public async Task<IEnumerable<ItenLancando>> Handle(GetLancamentosByTipoQuery request, CancellationToken cancellationToken)
        {
            // Busca os lançamentos pelo tipo informado no repositório
            var lancamentos = await _repository.GetByTipoAsync(request.Tipo, cancellationToken);

            // Converte os resultados para a lista de DTOs e retorna
            return _mapper.Map<IEnumerable<ItenLancando>>(lancamentos);
        }
    }
}
