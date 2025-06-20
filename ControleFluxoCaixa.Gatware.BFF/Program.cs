using Prometheus;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Carrega o arquivo de configuração do Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 🔌 Registra Ocelot
builder.Services.AddOcelot(builder.Configuration);

// 📊 Contador Prometheus
var requestCounter = Metrics.CreateCounter("api_requests_total", "Total de requisições recebidas");

// 🔍 Swagger (opcional: só aparece se usar controllers reais — não é seu caso)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

var app = builder.Build();

// Ativa Swagger se quiser exibir endpoints locais (irrelevante no seu caso)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 📈 Exposição de métricas Prometheus em /metrics
app.MapMetrics();

// 📦 Lê rotas do ocelot.json e cria redirecionamentos manuais
var configPath = Path.Combine(Directory.GetCurrentDirectory(), "ocelot.json");
var json = await File.ReadAllTextAsync(configPath);
var config = JsonSerializer.Deserialize<OcelotConfig>(json);

// 🔁 Redireciona cada rota conforme definida no ocelot.json
foreach (var route in config.Routes)
{
    app.Map(route.UpstreamPathTemplate, async (HttpContext context) =>
    {
        requestCounter.Inc(); // Incrementa contador Prometheus

        var downstreamHost = route.DownstreamHostAndPorts.First();
        var downstreamUrl = $"{route.DownstreamScheme}://{downstreamHost.Host}:{downstreamHost.Port}{route.DownstreamPathTemplate}";

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n🔁 Redirecionando para: {downstreamUrl}");
        Console.ResetColor();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        try
        {
            var response = await client.GetAsync(downstreamUrl);
            var content = await response.Content.ReadAsStringAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Resposta recebida:");
            Console.ResetColor();
            Console.WriteLine(content);

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(content);
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Erro ao chamar a API: {ex.Message}");
            Console.ResetColor();

            context.Response.StatusCode = (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError);
            await context.Response.WriteAsync($"Erro ao redirecionar a requisição: {ex.Message}");
        }
    });
}

// ▶️ Executa o Ocelot como middleware (fallback geral se desejar)
await app.UseOcelot();

// 🚀 Inicia a aplicação
app.Run();


// ==============================
// MODELOS USADOS PELO OCELOT
// ==============================

public class OcelotConfig
{
    public List<OcelotRoute> Routes { get; set; } = new();
}

public class OcelotRoute
{
    public string UpstreamPathTemplate { get; set; } = string.Empty;
    public string DownstreamPathTemplate { get; set; } = string.Empty;
    public string DownstreamScheme { get; set; } = "http";
    public List<DownstreamHostAndPort> DownstreamHostAndPorts { get; set; } = new();
}

public class DownstreamHostAndPort
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
}
