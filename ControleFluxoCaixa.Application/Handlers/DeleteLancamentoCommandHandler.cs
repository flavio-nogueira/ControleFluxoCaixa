using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.DTOs.Response;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Domain.Enums;
using ControleFluxoCaixa.Domain.Interfaces;
using ControleFluxoCaixa.Messaging.MessagingSettings;
using ControleFluxoCaixa.Messaging.Publishers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControleFluxoCaixa.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar o comando de exclusão de lançamentos.
    /// Ele realiza as seguintes operações:
    /// - Remove o lançamento do banco de dados.
    /// - Publica uma mensagem na fila RabbitMQ.
    /// - Invalida o cache de lançamentos, se necessário.
    /// </summary>
    public class DeleteLancamentoCommandHandler
        : IRequestHandler<DeleteLancamentoCommand, LancamentoResponseDto>
    {
        private readonly ILancamentoRepository _repository;
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteLancamentoCommandHandler> _logger;
        private readonly IRabbitMqPublisher<ItenLancando> _publisher;
        private readonly RabbitMqSettings _settings;

        /// <summary>
        /// Construtor com injeção de dependências.
        /// </summary>
        public DeleteLancamentoCommandHandler(
            ILancamentoRepository repository,
            ICacheService cache,
            ILogger<DeleteLancamentoCommandHandler> logger,
            IRabbitMqPublisher<ItenLancando> publisher,
            IOptions<RabbitMqSettings> settings)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
            _publisher = publisher;
            _settings = settings.Value;
        }

        /// <summary>
        /// Manipula a exclusão dos lançamentos informados.
        /// </summary>
        public async Task<LancamentoResponseDto> Handle(DeleteLancamentoCommand request, CancellationToken cancellationToken)
        {
            // Cria a resposta com a quantidade total de registros recebidos para exclusão
            var resposta = new LancamentoResponseDto
            {
                Registros = request.Ids.Count
            };

            // Se não foram informados IDs, retorna falha
            if (resposta.Registros == 0)
            {
                _logger.LogWarning("Nenhum ID recebido para exclusão.");
                resposta.Sucesso = false;
                resposta.Mensagem = "Informe ao menos um ID para exclusão.";
                return resposta;
            }

            // Itera sobre todos os IDs informados
            foreach (var (id, index) in request.Ids.Select((v, i) => (v, i)))
            {
                try
                {
                    // Busca o lançamento no banco de dados
                    var lancamento = await _repository.GetByIdAsync(id, cancellationToken);
                    if (lancamento == null)
                    {
                        // Se não encontrado, adiciona erro na resposta
                        resposta.Erros.Add(new LancamentoErroDto
                        {
                            Id = id,
                            Data = DateTime.UtcNow,
                            Valor = 0,
                            Descricao = $"Lançamento {id} não encontrado.",
                            Tipo = TipoLancamento.Debito,
                            Erro = "Registro inexistente"
                        });
                        continue;
                    }

                    // Tenta excluir o lançamento
                    var excluido = await _repository.DeleteAsync(id, cancellationToken);
                    if (!excluido)
                    {
                        // Se não conseguir excluir, registra erro
                        resposta.Erros.Add(new LancamentoErroDto
                        {
                            Id = id,
                            Data = lancamento.Data,
                            Valor = lancamento.Valor,
                            Descricao = lancamento.Descricao,
                            Tipo = lancamento.Tipo,
                            Erro = "Falha ao excluir no banco de dados"
                        });
                        continue;
                    }

                    // Adiciona o ID à lista de sucesso
                    resposta.Retorno.Add(id);

                    // Publica o evento de exclusão no RabbitMQ
                    var fila = _settings.GetSettingsFor(TipoFila.Exclusao);
                    var item = new ItenLancando
                    {
                        Id = lancamento.Id,
                        Data = lancamento.Data,
                        Valor = lancamento.Valor,
                        Descricao = lancamento.Descricao,
                        Tipo = lancamento.Tipo
                    };

                    await _publisher.PublishAsync(item, TipoFila.Exclusao, cancellationToken);
                    _logger.LogInformation("Publicado no RabbitMQ: lancamento.exclusao – {Id}", id);
                }
                catch (Exception ex)
                {
                    // Captura qualquer erro inesperado
                    _logger.LogError(ex, "Erro ao excluir lançamento {Id}", id);
                    resposta.Erros.Add(new LancamentoErroDto
                    {
                        Id = id,
                        Data = DateTime.UtcNow,
                        Valor = 0,
                        Descricao = "Erro ao tentar excluir",
                        Tipo = TipoLancamento.Debito,
                        Erro = ex.Message
                    });
                }
            }

            // Se pelo menos um item foi excluído com sucesso, invalida o cache
            if (resposta.Retorno.Any())
            {
                await _cache.RemoveAsync("lancamentos:all", cancellationToken);
                _logger.LogInformation("Cache 'lancamentos:all' invalidado após exclusão.");
            }

            // Define a mensagem e status final com base nos resultados
            resposta.Sucesso = resposta.Erros.Count == 0;
            resposta.Mensagem = resposta.Sucesso
                ? "Todos os lançamentos foram excluídos com sucesso."
                : resposta.Retorno.Any()
                    ? "Alguns lançamentos foram excluídos, outros falharam. Verifique os erros."
                    : "Nenhum lançamento foi excluído.";

            return resposta;
        }
    }
}
