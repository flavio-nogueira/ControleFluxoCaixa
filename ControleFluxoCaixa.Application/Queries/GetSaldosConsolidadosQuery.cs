using MediatR;
using ControleFluxoCaixa.Mongo.Documents;

public record GetSaldosConsolidadosQuery(DateTime De, DateTime Ate) : IRequest<List<SaldoDiarioConsolidado>>;
