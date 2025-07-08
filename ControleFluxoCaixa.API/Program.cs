// -----------------------------------------------------------------------------
// Program.cs  |  ControleFluxoCaixa.API
// -----------------------------------------------------------------------------

// Importa os DTOs da aplicação
using ControleFluxoCaixa.API.Middlewares;
using ControleFluxoCaixa.API.Tracing;
using ControleFluxoCaixa.Application.DTOs;

// Importa as configurações fortemente tipadas da aplicação
using ControleFluxoCaixa.Application.Settings.ControleFluxoCaixa.Application.Settings;

// Registra os serviços e dependências via Inversão de Controle (IoC)
using ControleFluxoCaixa.Infrastructure.IoC;

// Importa os inicializadores de observabilidade, como HealthChecks e métricas
using ControleFluxoCaixa.Infrastructure.IoC.Observability;

// HealthChecks do ASP.NET Core com saída para UI
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// RateLimiting nativo do .NET 7+
using Microsoft.AspNetCore.RateLimiting;

// Bibliotecas de métricas Prometheus
using Prometheus;

// Bibliotecas de logging estruturado Serilog
using Serilog;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Tipos relacionados ao rate limiting
using System.Threading.RateLimiting;
using Serilog.Enrichers.Span;

// -----------------------------------------------------------------------------
// CRIA O BUILDER DA APLICAÇÃO ASP.NET CORE
// -----------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// (Opcional) Continua mapeando a seção "Loki" se você usar esses valores em outro ponto
var lokiSettings = builder.Configuration.GetSection("Loki").Get<LokiSettings>();

// ============================
// CONFIGURAÇÃO DO SERILOG
// ============================

// Cria o logger global lendo a configuração única do arquivo JSON
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("Serialog/serilog.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"Serialog/serilog.{builder.Environment.EnvironmentName}.json", optional: true)
        .Build())
        .Enrich.WithSpan()
    .CreateLogger();

// Substitui o logger padrão do ASP.NET Core por Serilog
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .SetIsOriginAllowed(_ => true) // Libera qualquer origem, mesmo para credenciais
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Permite envio de cookies, headers de autenticação, etc.
    });
});

// ============================
// REGISTRO DE SERVIÇOS
// ============================

// Registra serviços e dependências da aplicação (Application, Infra, JWT, Swagger, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

// Registra HealthChecks e métricas com base nas configurações
builder.Services.AddObservability(builder.Configuration);

// Adiciona suporte a OpenTelemetry Tracing
builder.Services.AddOpenTelemetry() // Ativa o OpenTelemetry para este projeto
    .ConfigureResource(resource => // Define as informações básicas do serviço monitorado (nome, versão, ambiente, etc.)
    {
        var otelSection = builder.Configuration.GetSection("OpenTelemetry"); // Lê as configurações do appsettings.json (opcional)

        resource
            .AddService(
                serviceName: otelSection.GetValue<string>("ServiceName") ?? "ControleFluxoCaixa.API", // Nome do serviço (aparece nos dashboards)
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")    // Versão da aplicação
            .AddAttributes(new[] // Atributos extras que enriquecem as métricas/traces (como "ambiente de implantação")
            {
                new KeyValuePair<string, object>(
                    "deployment.environment", // Nome do atributo
                    otelSection.GetValue<string>("Environment") ?? builder.Environment.EnvironmentName // Valor (pega do appsettings ou do ambiente)
                )
            });
    })
    .WithTracing(tracing => // Ativa o rastreamento (Tracing) distribuído
    {
        tracing
            .AddAspNetCoreInstrumentation() // Captura requisições HTTP internas do ASP.NET Core (ex: Controllers, Middlewares, etc.)
            .AddHttpClientInstrumentation() // Captura chamadas feitas via HttpClient para outros serviços
            .AddSqlClientInstrumentation()  // Captura comandos SQL executados via SqlClient (ex: SELECT, INSERT, etc.)

            .AddOtlpExporter(opt => // Exporta os dados para um endpoint OTLP (OpenTelemetry Collector)
            {
                opt.Endpoint = new Uri("http://otel-collector:4317"); // Endereço do OTEL Collector via GRPC (porta 4317 é padrão)
                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // Define o protocolo GRPC (mais leve e eficiente)
            });
    });

// ============================
// RATE LIMITING
// ============================

// Lê as configurações da seção "RateLimiting" do appsettings.json
builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection("RateLimiting"));
var rateSettings = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingSettings>();

// Registra o RateLimiter como middleware global
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateSettings.PermitLimit,
                Window = TimeSpan.FromMinutes(rateSettings.WindowInMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = rateSettings.QueueLimit
            }));

    // Callback para logar requisições que excederam o limite
    options.OnRejected = (context, _) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit excedido para o IP: {IP}", context.HttpContext.Connection.RemoteIpAddress);
        return ValueTask.CompletedTask;
    };
});

// Política nomeada adicional, útil por rota
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("HourlyPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateSettings.PermitLimit,
                Window = TimeSpan.FromMinutes(rateSettings.WindowInMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = rateSettings.QueueLimit
            }));
});

// -----------------------------------------------------------------------------
// CONSTRUÇÃO DO APP
// -----------------------------------------------------------------------------
var app = builder.Build();

// Middleware de captura de exceções personalizadas
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Aplica migrações do banco de dados automaticamente e executa seeds
await MigrationInitializer.ApplyMigrationsAsync(app);

// Ativa o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Gera o Swagger JSON
    app.UseSwaggerUI(); // Interface gráfica do Swagger
}

// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// Middleware de enriquecimento de logs (endpoint, method, user, status)
app.UseMiddleware<LokiEnrichmentMiddleware>();

app.UseCors("AllowAll"); // <-- Aplica o CORS corretamente antes da autenticação

// Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Exposição de métricas Prometheus
app.UseMetricServer(); // `/metrics`
app.UseHttpMetrics();  // Histograma por endpoint

// CORS
//app.UseCors("CorsPolicy");
app.UseCors("AllowAll");

// Rate Limiting
app.UseRateLimiter();

// Controllers
app.MapControllers();

// Endpoints de Health Check
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

// Inicia o servidor
await app.RunAsync();

// Necessário para testes de integração com WebApplicationFactory
public partial class Program { }
