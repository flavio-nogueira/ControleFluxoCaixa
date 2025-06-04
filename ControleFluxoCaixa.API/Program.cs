// Importa as configurações de injeção de dependência personalizadas da aplicação
using ControleFluxoCaixa.Application.Settings.ControleFluxoCaixa.Application.Settings;
using ControleFluxoCaixa.Infrastructure.IoC;

// Importa o inicializador responsável por aplicar migrations e executar seeds no banco de dados
using ControleFluxoCaixa.Infrastructure.IoC.DataBase;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ControleFluxoCaixa.Infrastructure.IoC.Observability;

// Importa os pacotes para logs estruturados e métricas
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Security.Cryptography;

// Cria o builder da aplicação Web, responsável por configurar serviços e middlewares
var builder = WebApplication.CreateBuilder(args);

// Mapeia a seção "Loki" para a classe fortemente tipada LokiSettings
var lokiSettings = builder.Configuration.GetSection("Loki").Get<LokiSettings>()!;

// 1) Configura Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Define o nível mínimo global como Debug (pode ser alterado para Info em produção)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Ignora logs ruidosos do ASP.NET (apenas Warning ou superior)
    .Enrich.FromLogContext() // Inclui contexto como RequestId, usuário, etc., nos logs

    // --- Saída no console (formato JSON estruturado, bom para logs locais ou Docker)
    .WriteTo.Console(new RenderedCompactJsonFormatter())

    // --- Saída em arquivo com rotação diária e expurgo automático (últimos 7 dias apenas)
    .WriteTo.File(
        path: "logs-fallback/log-.json", // Nome base do arquivo com rotação por data
        formatter: new RenderedCompactJsonFormatter(), // Formato JSON compatível com Loki/Grafana
        rollingInterval: RollingInterval.Day, // Gera um novo arquivo por dia
        retainedFileCountLimit: 7, // Mantém apenas os últimos 7 arquivos
        shared: true // Permite múltiplas instâncias acessando o mesmo arquivo (caso raro em dev)
    )
    // Para Logs-buffer sera necessario criar uma rotina para fazer expurgo dentro ambiente exp: aqui muito obrigado pela Scripts\expurgo-logs-buffer.ps1
    // --- Envio durável para Loki via HTTP, com buffer em disco (alta resiliência)
    .WriteTo.DurableHttpUsingTimeRolledBuffers(
        requestUri: lokiSettings.Uri, // URL da API Loki (ex: http://loki:3100/loki/api/v1/push)
        bufferBaseFileName: lokiSettings.BufferPath, // Caminho base para arquivos de buffer (ex: "Logs-buffer/loki-buffer")
        period: TimeSpan.FromSeconds(lokiSettings.PeriodSeconds), // Intervalo de envio (ex: a cada 5 segundos)
        textFormatter: new RenderedCompactJsonFormatter() // Formato dos logs (JSON Loki-compatible)
    )
    .CreateLogger();

// 2) Substitui os loggers padrão do .NET pelo Serilog
builder.Logging
    .ClearProviders()         // Remove os loggers padrão do .NET (Console, Debug, etc.)
    .AddSerilog(dispose: true); // Adiciona o Serilog como único provedor de log

// 2) Aplica o Serilog como logger padrão
builder.Logging.ClearProviders().AddSerilog(dispose: true);

// 3) Registra todos os serviços necessários no container de injeção de dependência
// (como banco de dados, autenticação, JWT, Swagger, Seeds, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

// 2) Configura observabilidade (HealthChecks com MySQL)
builder.Services.AddObservability(builder.Configuration);

// 4) Constrói a aplicação ASP.NET com base nas configurações definidas acima
var app = builder.Build();

// 5) Aplica automaticamente as migrations pendentes no banco de dados de identidade
// e executa scripts de seed (como criar usuário admin) assim que a aplicação é iniciada
await MigrationInitializer.ApplyMigrationsAsync(app);

// 6) Ativa o middleware Swagger (documentação da API) apenas no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Gera o documento Swagger JSON
    app.UseSwaggerUI();      // Habilita a interface visual do Swagger (Swagger UI)
}

// 7) Redireciona automaticamente requisições HTTP para HTTPS
app.UseHttpsRedirection();

// 8) Adiciona o middleware de autenticação (JWT, Cookies, etc.)
app.UseAuthentication();

// 9) Adiciona o middleware de autorização (verifica permissões após autenticar)
app.UseAuthorization();

// 10) Middleware de telemetria para Prometheus (exposição em /metrics)
app.UseMetricServer();    // Exibe métricas Prometheus em /metrics
app.UseHttpMetrics();     // Coleta métricas por requisição HTTP

// 11) Mapeia os endpoints dos controllers para responder às rotas definidas na API
app.MapControllers();

// 12) Mapeia endpoints de health check e controllers
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 12) Inicia a aplicação de forma assíncrona, escutando as requisições HTTP
await app.RunAsync();

// Necessário para testes de integração com WebApplicationFactory
public partial class Program { }
