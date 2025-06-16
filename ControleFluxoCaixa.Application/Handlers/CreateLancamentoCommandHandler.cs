using AutoMapper;
using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs.Response;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Domain.Interfaces;
using ControleFluxoCaixa.Messaging.MessagingSettings;
using ControleFluxoCaixa.Messaging.Publishers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Manipulador para o comando de criação de múltiplos lançamentos.
    /// Responsável por persistir os dados no banco, invalidar o cache e publicar no RabbitMQ.
    /// </summary>
    public class CreateLancamentoCommandHandler
        : IRequestHandler<CreateLancamentoCommand, LancamentoResponseDto>
    {
        private readonly ILancamentoRepository _repository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateLancamentoCommandHandler> _logger;
        private readonly IRabbitMqPublisher<DTOs.Itens> _publisher;
        private readonly RabbitMqSettings _settings;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        public CreateLancamentoCommandHandler(
            ILancamentoRepository repository,
            ICacheService cache,
            IMapper mapper,
            ILogger<CreateLancamentoCommandHandler> logger,
            IRabbitMqPublisher<DTOs.Itens> publisher,
            IOptions<RabbitMqSettings> settings)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _publisher = publisher;
            _settings = settings.Value;
        }

        /// <summary>
        /// Lida com a criação de lançamentos:
        /// - Valida entrada
        /// - Persiste no banco
        /// - Publica no RabbitMQ
        /// - Invalida cache "lancamentos:all" se houver sucesso
        /// </summary>
        public async Task<LancamentoResponseDto> Handle(CreateLancamentoCommand request, CancellationToken cancellationToken)
        {
            var resposta = new LancamentoResponseDto
            {
                Registros = request.Itens?.Count ?? 0
            };

            if (resposta.Registros == 0)
            {
                _logger.LogWarning("Nenhum lançamento recebido para criação.");
                resposta.Sucesso = false;
                resposta.Mensagem = "É necessário enviar ao menos um lançamento.";
                return resposta;
            }

            foreach (var (dto, index) in request.Itens.Select((v, i) => (v, i)))
            {
                try
                {
                    var entidade = _mapper.Map<Domain.Entities.Lancamento>(dto);
                    await _repository.CreateAsync(entidade, cancellationToken);

                    _logger.LogInformation("Lançamento concluído com Sucesso – {Id}", entidade.Id);

                    resposta.Retorno.Add(entidade.Id);

                    // Obtém a configuração da fila 
                    var fila = _settings.GetSettingsFor(TipoFila.Inclusao);

                    // Publica a mensagem no RabbitMQ 
                    await _publisher.PublishAsync(dto, TipoFila.Inclusao, cancellationToken);
                    _logger.LogInformation("Publicado no RabbitMQ: lancamento.inclusao – {Id}", entidade.Id);


                }
                catch (Exception ex)
                {
                    string payload = "(inválido)";
                    try { payload = JsonSerializer.Serialize(dto); } catch { }

                    _logger.LogError(ex, "Erro ao criar lançamento no índice {Index}. Payload: {Payload}", index, payload);

                    resposta.Erros.Add(new LancamentoErroDto
                    {
                        Id = null,
                        Data = dto.Data,
                        Valor = dto.Valor,
                        Descricao = dto.Descricao,
                        Tipo = dto.Tipo,
                        Erro = ex.Message
                    });
                }
            }

            // Invalida o cache global apenas se houve sucesso em ao menos um registro
            if (resposta.Retorno.Any())
            {
                await _cache.RemoveAsync("lancamentos:all", cancellationToken);
                _logger.LogInformation("Cache 'lancamentos:all' invalidado após criação.");
            }

            // Define o status geral da operação
            if (resposta.Retorno.Count == 0)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Nenhum lançamento foi criado com sucesso.";
            }
            else if (resposta.Erros.Count == 0)
            {
                resposta.Sucesso = true;
                resposta.Mensagem = $"Todos os {resposta.Registros} lançamentos foram criados com sucesso.";
            }
            else
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Alguns lançamentos foram criados, outros falharam. Verifique os erros.";
            }

            return resposta;
        }
    }
}
