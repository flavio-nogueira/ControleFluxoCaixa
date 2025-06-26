// Importa os DTOs da aplicação
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
using Serilog.Events;
using Serilog.Formatting.Compact;

// Tipos relacionados ao rate limiting
using System.Threading.RateLimiting;

// Cria o builder da aplicação ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Mapeia a seção "Loki" do appsettings.json para a classe fortemente tipada "LokiSettings"
var lokiSettings = builder.Configuration.GetSection("Loki").Get<LokiSettings>()!;

// ============================
// CONFIGURAÇÃO DO SERILOG
// ============================

// Inicializa e configura o logger Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Log mínimo = Debug (pode ser Info em produção)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Reduz ruído dos logs internos do .NET
    .Enrich.FromLogContext() // Adiciona contexto de requisição (ex: TraceId, IP, etc.)

    // Exibe logs estruturados no console (JSON compacto)
    .WriteTo.Console(new RenderedCompactJsonFormatter())

    // Grava logs em arquivo com rotação diária e retenção de 7 dias
    .WriteTo.File(
        path: "logs-fallback/log-.json",
        formatter: new RenderedCompactJsonFormatter(),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true
    )

    // Envia logs de forma resiliente para o Loki, com buffer no disco
    .WriteTo.DurableHttpUsingTimeRolledBuffers(
        requestUri: lokiSettings.Uri,
        bufferBaseFileName: lokiSettings.BufferPath,
        period: TimeSpan.FromSeconds(lokiSettings.PeriodSeconds),
        textFormatter: new RenderedCompactJsonFormatter()
    )
    .CreateLogger();

// Remove os loggers padrões e adiciona o Serilog como provedor exclusivo de logs
builder.Logging.ClearProviders().AddSerilog(dispose: true);

// ============================
// REGISTRO DE SERVIÇOS
// ============================

// Registra serviços e dependências da aplicação (Application, Infra, JWT, Swagger, etc.)
builder.Services.AddApplicationServices(builder.Configuration);

// Registra HealthChecks e métricas com base nas configurações
builder.Services.AddObservability(builder.Configuration);

// ============================
// RATE LIMITING
// ============================

// Lê as configurações da seção "RateLimiting" do appsettings.json
builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection("RateLimiting"));
var settings = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingSettings>();

// Registra o RateLimiter como middleware global
builder.Services.AddRateLimiter(options =>
{
    // Define um limitador global usando janela fixa (FixedWindow)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", // IP do cliente
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,               // Quantidade máxima de requisições
                Window = TimeSpan.FromMinutes(settings.WindowInMinutes), // Janela de tempo para o limite
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // Ordem de espera na fila (FIFO)
                QueueLimit = settings.QueueLimit                  // Tamanho da fila de espera
            }));

    // Callback para logar requisições que excederam o limite
    options.OnRejected = (context, _) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit excedido para o IP: {IP}", context.HttpContext.Connection.RemoteIpAddress);
        return ValueTask.CompletedTask;
    };
});

// Política nomeada adicional, útil se desejar usar `.RequireRateLimiting("HourlyPolicy")` por rota
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("HourlyPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromMinutes(settings.WindowInMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = settings.QueueLimit
            }));
});

// ============================
// CONSTRUÇÃO DO APP
// ============================

var app = builder.Build();

// Middleware de captura de exceções personalizadas (deve vir antes dos handlers da pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Aplica migrações do banco de dados automaticamente e executa seeds (como criação do admin)
await MigrationInitializer.ApplyMigrationsAsync(app);

// Ativa o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Gera o Swagger JSON
    app.UseSwaggerUI(); // Interface gráfica do Swagger
}

// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// Habilita autenticação e autorização (com suporte a JWT, cookies, etc.)
app.UseAuthentication();
app.UseAuthorization();

// Exposição das métricas Prometheus
app.UseMetricServer(); // `/metrics` endpoint Prometheus
app.UseHttpMetrics();  // Coleta estatísticas por endpoint HTTP

// Aplica política de CORS definida no projeto
app.UseCors("CorsPolicy");

// Ativa o middleware de Rate Limiting para toda a aplicação (com base em política global)
app.UseRateLimiter();

// Mapeia todos os endpoints de controllers da API
app.MapControllers();

// Endpoint de health check para verificar se a aplicação está viva
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Endpoint de health check para readiness (serviços externos prontos)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Inicializa o servidor web e começa a escutar requisições
await app.RunAsync();

// Necessário para permitir testes de integração com WebApplicationFactory
public partial class Program { }
