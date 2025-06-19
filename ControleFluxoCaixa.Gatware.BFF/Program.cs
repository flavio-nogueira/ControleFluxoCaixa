using Prometheus;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Add Prometheus metrics counter
var requestCounter = Metrics.CreateCounter("api_requests_total", "Total de requisições recebidas");

// Add Swagger for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

var app = builder.Build();

// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Setup route to increase the Prometheus counter for every request
app.MapGet("/api/gateway", (HttpContext context) =>
{
    requestCounter.Inc(); // Increment the counter for every request received
    return Results.Ok("Requisição processada");
});

// Endpoint for Prometheus to scrape metrics
//app.MapGet("/metrics", (HttpContext context) =>
//{
//    // Collect the metrics from the default registry
//    var metrics = Metrics.DefaultRegistry.CollectAll();  // Collect all metrics
//    context.Response.ContentType = "text/plain";  // Set content type to Prometheus format

//    foreach (var metricFamily in metrics)
//    {
//        context.Response.WriteAsync(metricFamily.ToString()); // Write the collected metrics to response
//    }

//    return Task.CompletedTask;
//});

// Setup Ocelot for routing requests


// Custom middleware to log request details
app.Use(async (context, next) =>
{
    // Logando detalhes da requisição
    var request = context.Request;
    Console.WriteLine($"Método: {request.Method}");
    Console.WriteLine($"URL: {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
    Console.WriteLine($"Requisição recebida: {context.Request.Method} {context.Request.Path}");  // Loga o método e o caminho

    // Logando os cabeçalhos da requisição
    foreach (var header in request.Headers)
    {
        Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    }

    // Lê as rotas configuradas no Ocelot
    var ocelotConfig = builder.Configuration.GetSection("Routes").Get<List<RouteConfig>>();

    // Log para verificar as rotas carregadas
    Console.WriteLine("Rotas carregadas do Ocelot:");
    foreach (var r in ocelotConfig)
    {
        Console.WriteLine($"UpstreamPathTemplate: {r.UpstreamPathTemplate}, DownstreamPathTemplate: {r.DownstreamPathTemplate}");
    }

    // Captura a rota desejada, ajustando o nome da variável para evitar conflito
    var matchingRoute = ocelotConfig?.FirstOrDefault(r => r.UpstreamPathTemplate == "/gateway/lancamento/getall");

    if (matchingRoute != null)
    {
        // Construindo a URL downstream com base na configuração do Ocelot
        var downstreamUrl = $"https://host.docker.internal:5001{matchingRoute.DownstreamPathTemplate}";

        Console.WriteLine($"URL de destino: {downstreamUrl}");

        // Cria um handler que ignora a validação do certificado SSL (somente para desenvolvimento)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        try
        {
            var response = await client.GetAsync(downstreamUrl); // faz a requisição GET
            response.EnsureSuccessStatusCode(); // lança exceção se não for sucesso

            var responseBody = await response.Content.ReadAsStringAsync(); // Lê a resposta
            Console.WriteLine(responseBody); // Exibe o conteúdo da resposta
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erro ao chamar a API: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Nenhuma rota encontrada para o caminho solicitado.");
    }


    // Incrementando o contador de requisições para Prometheus
    requestCounter.Inc(); // Incrementa o contador para cada requisição recebida

    // Passando a requisição para o próximo middleware
    await next.Invoke();
});

await app.UseOcelot(); // Ocelot middleware for API Gateway routing
app.Run();


public class RouteConfig
{
    public string UpstreamPathTemplate { get; set; }
    public List<string> UpstreamHttpMethod { get; set; }
    public string DownstreamPathTemplate { get; set; }
    public string DownstreamScheme { get; set; }
    public List<HostAndPort> DownstreamHostAndPorts { get; set; }
}

public class HostAndPort
{
    public string Host { get; set; }
    public int Port { get; set; }
}