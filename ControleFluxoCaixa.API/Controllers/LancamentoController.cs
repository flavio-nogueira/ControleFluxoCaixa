using Amazon.Runtime.Internal.Util;
using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs.Response;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Mongo.Documents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using Prometheus;
using Serilog;
using ILogger = Serilog.ILogger; 

namespace ControleFluxoCaixa.API.Controllers
{
    //[ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LancamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        // Métricas Prometheus
        private static readonly Counter CreateCounter = Metrics.CreateCounter(
            "controlefluxocaixa_lancamento_create_requests_total",
            "Contador de requisições para criação de lançamentos");

        private static readonly Counter GetAllCounter = Metrics.CreateCounter(
            "controlefluxocaixa_lancamento_getall_requests_total",
            "Contador de requisições para retornar todos os lançamentos");

        private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
            "controlefluxocaixa_request_duration_seconds",
            "Duração das requisições em segundos",
            new HistogramConfiguration { LabelNames = new[] { "method", "endpoint", "status" } });

        // Política de retry com backoff exponencial
        private static readonly AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
                onRetry: (ex, ts, retryCount, ctx) =>
                {
                    Log.Warning("Retry {RetryCount} after {Delay}ms due to {Exception}", retryCount, ts.TotalMilliseconds, ex.Message);
                });

        public LancamentoController(IMediator mediator, ICacheService cacheService)
        {
            _mediator = mediator;
            _cacheService = cacheService;
            _logger = Log.ForContext<LancamentoController>();
        }

        /// <summary>
        /// Retorna todos os lançamentos em cache ou banco de dados.
        /// </summary>
        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<LancamentoResponseDto>> GetAll(CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("GET", "GetAll", "").NewTimer();
            GetAllCounter.Inc();

            try
            {
                var lancamentos = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        "lancamentos:all",
                        async () => await _mediator.Send(new GetAllLancamentosQuery(), cancellationToken),
                        TimeSpan.FromMinutes(10),
                        cancellationToken));

                var response = new LancamentoResponseDto
                {
                    Mensagem = "Consulta realizada com sucesso",
                    Sucesso = true,
                    Registros = lancamentos.Count(),
                    Retorno = lancamentos.Cast<object>().ToList()

                };

                timer.ObserveDuration();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro interno ao recuperar todos os lançamentos");

                var responseErro = new LancamentoResponseDto
                {
                    Mensagem = "Falha ao consultar lançamentos",
                    Sucesso = false,
                    Registros = 0,
                    Erros = new List<LancamentoErroDto>
        {
            new LancamentoErroDto
            {
                Id = null,
                Data = DateTime.UtcNow,
                Valor = 0,
                Descricao = "Erro na consulta de lançamentos",
                Tipo = TipoLancamento.Debito, // ou outro valor padrão
                Erro = ex.Message
            }
        }
                };

                timer.ObserveDuration();
                return StatusCode(500, responseErro);
            }
        }

        /// <summary>
        /// Retorna os lançamentos de um determinado tipo (Crédito ou Débito).
        /// </summary>
        [HttpGet("GetByTipo/{tipo}")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<LancamentoResponseDto>> GetByTipo(TipoLancamento tipo, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("GET", "GetByTipo", "").NewTimer();

            try
            {
                if (!Enum.IsDefined(typeof(TipoLancamento), tipo))
                {
                    _logger.Warning("Tipo de lançamento inválido recebido: {Tipo}", tipo);
                    timer.ObserveDuration();

                    return BadRequest(new LancamentoResponseDto
                    {
                        Mensagem = "Tipo de lançamento inválido.",
                        Sucesso = false,
                        Registros = 0,
                        Retorno = new List<object>(),
                        Erros = new List<LancamentoErroDto>
                {
                    new LancamentoErroDto
                    {
                        Id = null,
                        Data = DateTime.UtcNow,
                        Valor = 0,
                        Descricao = "Tipo informado não é válido.",
                        Tipo = tipo,
                        Erro = "Enum inválido"
                    }
                }
                    });
                }

                var lancamentos = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        $"lancamentos:tipo:{tipo}",
                        async () => await _mediator.Send(new GetLancamentosByTipoQuery(tipo), cancellationToken),
                        TimeSpan.FromMinutes(10),
                        cancellationToken));

                var response = new LancamentoResponseDto
                {
                    Mensagem = "Consulta realizada com sucesso",
                    Sucesso = true,
                    Registros = lancamentos.Count(),
                    Retorno = lancamentos.Cast<object>().ToList()
                };

                timer.ObserveDuration();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro interno ao recuperar lançamentos por tipo: {Tipo}", tipo);

                var responseErro = new LancamentoResponseDto
                {
                    Mensagem = "Erro ao consultar lançamentos por tipo",
                    Sucesso = false,
                    Registros = 0,
                    Retorno = new List<object>(),
                    Erros = new List<LancamentoErroDto>
            {
                new LancamentoErroDto
                {
                    Id = null,
                    Data = DateTime.UtcNow,
                    Valor = 0,
                    Descricao = $"Erro ao consultar lançamentos do tipo {tipo}",
                    Tipo = tipo,
                    Erro = ex.Message
                }
            }
                };

                timer.ObserveDuration();
                return StatusCode(500, responseErro);
            }
        }

        /// <summary>
        /// Retorna o lançamento com base no ID informado.
        /// </summary>
        [HttpGet("GetById/{id:guid}", Name = "GetLancamentoById")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<LancamentoResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("GET", "GetById", "").NewTimer();

            try
            {
                var lancamento = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        $"lancamento:{id}",
                        async () => await _mediator.Send(new GetLancamentoByIdQuery(id), cancellationToken),
                        TimeSpan.FromMinutes(10),
                        cancellationToken));

                if (lancamento == null)
                {
                    _logger.Information("Lançamento não encontrado para o ID: {Id}", id);
                    timer.ObserveDuration();

                    return NotFound(new LancamentoResponseDto
                    {
                        Mensagem = "Lançamento não encontrado",
                        Sucesso = false,
                        Registros = 0,
                        Retorno = new List<object>(),
                        Erros = new List<LancamentoErroDto>
                {
                    new LancamentoErroDto
                    {
                        Id = id,
                        Data = DateTime.UtcNow,
                        Valor = 0,
                        Descricao = $"Lançamento com ID {id} não encontrado",
                        Tipo = TipoLancamento.Debito,
                        Erro = "Recurso não encontrado"
                    }
                }
                    });
                }

                var response = new LancamentoResponseDto
                {
                    Mensagem = "Consulta realizada com sucesso",
                    Sucesso = true,
                    Registros = 1,
                    Retorno = new List<object> { lancamento }
                };

                timer.ObserveDuration();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro interno ao recuperar lançamento por ID: {Id}", id);

                var responseErro = new LancamentoResponseDto
                {
                    Mensagem = "Erro ao consultar lançamento",
                    Sucesso = false,
                    Registros = 0,
                    Retorno = new List<object>(),
                    Erros = new List<LancamentoErroDto>
            {
                new LancamentoErroDto
                {
                    Id = id,
                    Data = DateTime.UtcNow,
                    Valor = 0,
                    Descricao = $"Erro ao consultar o lançamento com ID {id}",
                    Tipo = TipoLancamento.Debito,
                    Erro = ex.Message
                }
            }
                };

                timer.ObserveDuration();
                return StatusCode(500, responseErro);
            }
        }

        /// <summary>
        /// Cria um ou mais lançamentos e retorna o ID do principal.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(Guid), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateLancamentoCommand command, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("POST", "Create", "").NewTimer();
            CreateCounter.Inc();

            var id = await _mediator.Send(command, cancellationToken); // se der erro, middleware cuida

            await _cacheService.RemoveAsync("lancamentos:all", cancellationToken);

            var location = Url.Link("GetLancamentoById", new { id });
            _logger.Information("Lançamento criado com sucesso: {Id}", id);
            timer.ObserveDuration();

            return Created(location!, id);
        }

        /// <summary>
        /// Exclui um lançamento com base no ID informado.
        /// </summary>
        [HttpDelete("DeleteMany")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        [ProducesResponseType(typeof(LancamentoResponseDto), 400)]
        [ProducesResponseType(typeof(LancamentoResponseDto), 500)]
        public async Task<ActionResult<LancamentoResponseDto>> DeleteMany([FromBody] List<Guid> ids, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("DELETE", "DeleteMany", "").NewTimer();

            if (ids == null || !ids.Any())
            {
                var erro = new LancamentoResponseDto
                {
                    Sucesso = false,
                    Mensagem = "É necessário informar ao menos um ID para exclusão.",
                    Registros = 0
                };

                return BadRequest(erro);
            }

            try
            {
                var result = await _mediator.Send(new DeleteLancamentoCommand { Ids = ids }, cancellationToken);

                // Remove cache geral e individual dos IDs excluídos
                if (result.Retorno.Any())
                {
                    await _cacheService.RemoveAsync("lancamentos:all", cancellationToken);
                    foreach (var id in result.Retorno)
                        await _cacheService.RemoveAsync($"lancamento:{id}", cancellationToken);
                }

                timer.ObserveDuration();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro interno ao excluir lançamentos.");

                var erroResponse = new LancamentoResponseDto
                {
                    Sucesso = false,
                    Mensagem = "Erro ao excluir lançamentos.",
                    Registros = ids.Count,
                    Erros = ids.Select(id => new LancamentoErroDto
                    {
                        Id = id,
                        Data = DateTime.UtcNow,
                        Valor = 0,
                        Descricao = "Erro ao excluir lançamento",
                        Tipo = TipoLancamento.Debito,
                        Erro = ex.Message
                    }).ToList()
                };

                timer.ObserveDuration();
                return StatusCode(500, erroResponse);
            }
        }

        /// <summary>
        /// Retorna o saldo diário consolidado entre as datas informadas, com cache.
        /// </summary>
        [HttpGet("saldos")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        [ProducesResponseType(typeof(LancamentoResponseDto), 500)]
        public async Task<ActionResult<LancamentoResponseDto>> GetSaldos(
        [FromQuery] DateTime de,
        [FromQuery] DateTime ate,
        CancellationToken ct)
        {
            var timer = RequestDuration.WithLabels("GET", "GetSaldosConsolidados", "").NewTimer();
            GetAllCounter.Inc();

            if (de > ate)
            {
                return BadRequest(new LancamentoResponseDto
                {
                    Mensagem = "Parâmetro 'de' não pode ser maior que 'ate'.",
                    Sucesso = false,
                    Registros = 0
                });
            }

            var cacheKey = $"saldos:{de:yyyyMMdd}:{ate:yyyyMMdd}";

            try
            {
                var saldos = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        cacheKey,
                        async () => await _mediator.Send(new GetSaldosConsolidadosQuery(de, ate), ct),
                        TimeSpan.FromMinutes(10),
                        ct));

                return Ok(new LancamentoResponseDto
                {
                    Mensagem = "Consulta realizada com sucesso",
                    Sucesso = true,
                    Registros = saldos.Count,
                    Retorno = saldos.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erro ao consultar saldos consolidados");

                return StatusCode(500, new LancamentoResponseDto
                {
                    Mensagem = "Erro interno",
                    Sucesso = false,
                    Registros = 0,
                    Erros = new List<LancamentoErroDto>
            {
                new()
                {
                    Data = DateTime.UtcNow,
                    Descricao = $"Erro ao consultar saldos de {de:yyyy-MM-dd} a {ate:yyyy-MM-dd}",
                    Erro = ex.Message,
                    Tipo = TipoLancamento.Debito
                }
            }
                });
            }
        }

        // -----------------------------------------------------------------------------
        // 1) Retorna TODAS as páginas de lançamentos (paginado)
        //     Ex: /api/Lancamento/GetAllPaginado?page=1&pageSize=20
        // -----------------------------------------------------------------------------
        [HttpGet("GetAllPaginado")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        public async Task<ActionResult<LancamentoResponseDto>> GetAllPaginado(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var timer = RequestDuration.WithLabels("GET", "GetAllPaginado", "").NewTimer();
            GetAllCounter.Inc();

            try
            {
                var allLanc = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        "lancamentos:all",
                        async () => await _mediator.Send(new GetAllLancamentosQuery(), cancellationToken),
                        TimeSpan.FromMinutes(10),
                        cancellationToken));

                var total = allLanc.Count();
                var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

                var resultado = allLanc
                    .OrderByDescending(l => l.Data)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Cast<object>()
                    .ToList();

                timer.ObserveDuration();

                return Ok(new LancamentoResponseDto
                {
                    Mensagem = "Consulta paginada com sucesso",
                    Sucesso = true,
                    Registros = total,
                    PaginaAtual = page,
                    TotalPaginas = totalPaginas,
                    Retorno = resultado
                });
            }
            catch (Exception ex)
            {
                timer.ObserveDuration();
                _logger.Error(ex, "Erro interno em GetAllPaginado");

                return StatusCode(500, new LancamentoResponseDto
                {
                    Mensagem = "Erro interno",
                    Sucesso = false,
                    Registros = 0
                });
            }
        }

        // -----------------------------------------------------------------------------
        // 2) Retorna lançamentos por Tipo (Crédito/Débito) de forma paginada
        //     Ex: /api/Lancamento/GetByTipoPaginado/1?page=2&pageSize=10
        // -----------------------------------------------------------------------------
        [HttpGet("GetByTipoPaginado/{tipo}")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        public async Task<ActionResult<LancamentoResponseDto>> GetByTipoPaginado(
            TipoLancamento tipo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var timer = RequestDuration.WithLabels("GET", "GetByTipoPaginado", "").NewTimer();

            if (!Enum.IsDefined(typeof(TipoLancamento), tipo))
            {
                return BadRequest(new LancamentoResponseDto
                {
                    Mensagem = "Tipo inválido.",
                    Sucesso = false,
                    Registros = 0
                });
            }

            try
            {
                var lancamentos = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        $"lancamentos:tipo:{tipo}",
                        async () => await _mediator.Send(new GetLancamentosByTipoQuery(tipo), cancellationToken),
                        TimeSpan.FromMinutes(10),
                        cancellationToken));

                var total = lancamentos.Count();
                var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

                var resultado = lancamentos
                    .OrderByDescending(l => l.Data)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Cast<object>()
                    .ToList();

                timer.ObserveDuration();

                return Ok(new LancamentoResponseDto
                {
                    Mensagem = "Consulta paginada com sucesso",
                    Sucesso = true,
                    Registros = total,
                    PaginaAtual = page,
                    TotalPaginas = totalPaginas,
                    Retorno = resultado
                });
            }
            catch (Exception ex)
            {
                timer.ObserveDuration();
                _logger.Error(ex, "Erro interno em GetByTipoPaginado");

                return StatusCode(500, new LancamentoResponseDto
                {
                    Mensagem = "Erro interno",
                    Sucesso = false,
                    Registros = 0
                });
            }
        }

        // -----------------------------------------------------------------------------
        // 3) Retorna saldos consolidados paginados
        //     Ex: /api/Lancamento/saldosPaginado?de=2025-01-01&ate=2025-06-30&page=1&pageSize=15
        // -----------------------------------------------------------------------------
        [HttpGet("saldosPaginado")]
        [ProducesResponseType(typeof(LancamentoResponseDto), 200)]
        public async Task<ActionResult<LancamentoResponseDto>> GetSaldosPaginado(
            [FromQuery] DateTime de,
            [FromQuery] DateTime ate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var timer = RequestDuration.WithLabels("GET", "GetSaldosPaginado", "").NewTimer();

            if (de > ate)
            {
                return BadRequest(new LancamentoResponseDto
                {
                    Mensagem = "'de' não pode ser maior que 'ate'.",
                    Sucesso = false,
                    Registros = 0
                });
            }

            try
            {
                var cacheKey = $"saldos:{de:yyyyMMdd}:{ate:yyyyMMdd}";
                var saldos = await RetryPolicy.ExecuteAsync(() =>
                    _cacheService.GetOrSetAsync(
                        cacheKey,
                        async () => await _mediator.Send(new GetSaldosConsolidadosQuery(de, ate), ct),
                        TimeSpan.FromMinutes(10),
                        ct));

                var total = saldos.Count;
                var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

                var resultado = saldos
                    .OrderBy(s => s.Date) // string no formato yyyy-MM-dd
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Cast<object>()
                    .ToList();

                timer.ObserveDuration();

                return Ok(new LancamentoResponseDto
                {
                    Mensagem = "Consulta paginada com sucesso",
                    Sucesso = true,
                    Registros = total,
                    PaginaAtual = page,
                    TotalPaginas = totalPaginas,
                    Retorno = resultado
                });
            }
            catch (Exception ex)
            {
                timer.ObserveDuration();
                _logger.Error(ex, "Erro interno em GetSaldosPaginado");

                return StatusCode(500, new LancamentoResponseDto
                {
                    Mensagem = "Erro interno",
                    Sucesso = false,
                    Registros = 0
                });
            }
        }

    }
}
