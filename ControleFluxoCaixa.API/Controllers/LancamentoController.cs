using ControleFluxoCaixa.Application.Commands;
using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.DTOs.Response;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Domain.Entities;
using ControleFluxoCaixa.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using Prometheus;
using Serilog;
using ILogger = Serilog.ILogger; // Resolve ambiguidade explicitamente

namespace ControleFluxoCaixa.API.Controllers
{
    //[ApiController]
    [Route("api/[controller]")]
   // [Authorize]
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

        //public async Task<ActionResult<Guid>> Create([FromBody] CreateLancamentoCommand command, CancellationToken cancellationToken)
        //{
        //    var timer = RequestDuration.WithLabels("POST", "Create", "").NewTimer();
        //    CreateCounter.Inc();
        //    try
        //    {
        //        if (command == null)
        //        {
        //            _logger.Warning("Dados de lançamento inválidos recebidos: comando nulo");
        //            timer.ObserveDuration();
        //            return BadRequest("Dados de lançamento inválidos.");
        //        }

        //        var id = await _mediator.Send(command, cancellationToken);

        //        // Remove o cache geral após criação
        //        await _cacheService.RemoveAsync("lancamentos:all", cancellationToken);

        //        var location = Url.Link("GetLancamentoById", new { id });
        //        _logger.Information("Lançamento criado com sucesso: {Id}", id);
        //        timer.ObserveDuration();
        //        return Created(location!, id);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex, "Erro interno ao criar lançamento");
        //        timer.ObserveDuration();
        //        return StatusCode(500, "Ocorreu um erro interno ao criar o lançamento.");
        //    }
        //}


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


    }
}
