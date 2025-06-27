using ControleFluxoCaixa.Mongo.Documents;
using ControleFluxoCaixa.MongoDB.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar a consulta de saldos diários consolidados entre duas datas.
    /// </summary>
    public class GetSaldosConsolidadosQueryHandler : IRequestHandler<GetSaldosConsolidadosQuery, List<SaldoDiarioConsolidado>>
    {
        private readonly ISaldoDiarioConsolidadoRepository _repo;
        private readonly ILogger<GetSaldosConsolidadosQueryHandler> _logger;

        /// <summary>
        /// Construtor que injeta o repositório de saldos e o logger.
        /// </summary>
        public GetSaldosConsolidadosQueryHandler(ISaldoDiarioConsolidadoRepository repo, ILogger<GetSaldosConsolidadosQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// Manipula a requisição, consultando o MongoDB entre duas datas fornecidas.
        /// </summary>
        /// <param name="request">Contém as datas "De" e "Até" para filtragem.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação assíncrona.</param>
        /// <returns>Lista de saldos diários consolidados no intervalo de datas.</returns>
        public async Task<List<SaldoDiarioConsolidado>> Handle(GetSaldosConsolidadosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando consulta de saldos entre {DataInicial} e {DataFinal}.", request.De.Date.ToShortDateString(), request.Ate.Date.ToShortDateString());

            var saldos = await _repo.GetBetweenAsync(request.De.Date, request.Ate.Date, cancellationToken);

            _logger.LogInformation("Consulta concluída. {Quantidade} registros encontrados.", saldos.Count);

            return saldos;
        }
    }
}
