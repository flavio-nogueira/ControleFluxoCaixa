using Prometheus;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Carrega o ocelot.json (com SwaggerEndPoints incluído)
builder.Configuration
    .AddJsonFile("Configuration/ocelot.json")
    .AddJsonFile("Configuration/ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);

// Registra Ocelot e SwaggerForOcelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Prometheus
var requestCounter = Metrics.CreateCounter("api_requests_total", "Contador BFF");

// Adiciona SwaggerGen apenas para compatibilidade (não usaremos UseSwaggerUI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ControleFluxoCaixa.Gatware.BFF",
        Version = "v1"
    });
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var httpClientHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    using var client = new HttpClient(httpClientHandler);

    try
    {
        var url = "https://controlefluxocaixa_api/swagger/v1/swagger.json";
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API controlefluxocaixa_api está acessível.");
            Console.WriteLine("Conteúdo retornado:");
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine($"API respondeu com erro: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao testar API controlefluxocaixa_api: {ex.Message}");
    }
});

// Prometheus
app.Use(async (context, next) =>
{
    requestCounter.Inc();
    await next();
});
app.MapMetrics();

// Swagger das rotas via Ocelot 
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.RoutePrefix = "swagger"; // → /swagger/index.html
});
app.MapGet("/teste-api", async context =>
{
    context.Response.ContentType = "text/plain";

    try
    {
        var url = "https://controlefluxocaixa_api/swagger/v1/swagger.json";
        Console.WriteLine($"➡️ Iniciando chamada para: {url}");

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");

        await context.Response.WriteAsync($"Conectado a: {url}\n\n{content[..Math.Min(content.Length, 500)]}");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERRO: {ex.Message}");
        Console.ResetColor();

        await context.Response.WriteAsync($"Erro: {ex.Message}\n\nStack:\n{ex.StackTrace}");
    }
});



// Executa Ocelot
await app.UseOcelot();

app.Run();
