using Prometheus;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ✅ Carrega os dois arquivos corretamente
builder.Configuration
    .AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("Configuration/ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);

// ✅ Adiciona Ocelot e SwaggerForOcelot normalmente (apenas 2 parâmetros)
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// 🔧 Swagger básico (necessário)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ControleFluxoCaixa.Gatware.BFF",
        Version = "v1"
    });
});

// 🔧 Prometheus contador
var requestCounter = Metrics.CreateCounter("api_requests_total", "Contador de requisições");

var app = builder.Build();

// Middleware Prometheus
app.Use(async (context, next) =>
{
    requestCounter.Inc();
    await next();
});
app.MapMetrics();

app.UseStaticFiles(); // ⬅️ OBRIGATÓRIO para servir wwwroot/swagger/v1/swagger.json


// ✅ Interface Swagger via Ocelot
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.RoutePrefix = "swagger"; // → http://localhost:{porta}/swagger/index.html
});

// Inicializa Ocelot
await app.UseOcelot();
app.Run();
