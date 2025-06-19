using Prometheus;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Add Prometheus metrics counter
var requestCounter = Metrics.CreateCounter("api_requests_total", "Total de requisi��es recebidas");

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
    return Results.Ok("Requisi��o processada");
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
    // Logando detalhes da requisi��o
    var request = context.Request;
    Console.WriteLine($"M�todo: {request.Method}");
    Console.WriteLine($"URL: {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
    Console.WriteLine($"Requisi��o recebida: {context.Request.Method} {context.Request.Path}");  // Loga o m�todo e o caminho

    // Logando os cabe�alhos da requisi��o
    foreach (var header in request.Headers)
    {
        Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    }

    // L� as rotas configuradas no Ocelot
    var ocelotConfig = builder.Configuration.GetSection("Routes").Get<List<RouteConfig>>();

    // Log para verificar as rotas carregadas
    Console.WriteLine("Rotas carregadas do Ocelot:");
    foreach (var r in ocelotConfig)
    {
        Console.WriteLine($"UpstreamPathTemplate: {r.UpstreamPathTemplate}, DownstreamPathTemplate: {r.DownstreamPathTemplate}");
    }

    // Captura a rota desejada, ajustando o nome da vari�vel para evitar conflito
    var matchingRoute = ocelotConfig?.FirstOrDefault(r => r.UpstreamPathTemplate == "/gateway/lancamento/getall");

    if (matchingRoute != null)
    {
        // Construindo a URL downstream com base na configura��o do Ocelot
        var downstreamUrl = $"https://host.docker.internal:5001{matchingRoute.DownstreamPathTemplate}";

        Console.WriteLine($"URL de destino: {downstreamUrl}");

        // Cria um handler que ignora a valida��o do certificado SSL (somente para desenvolvimento)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        try
        {
            var response = await client.GetAsync(downstreamUrl); // faz a requisi��o GET
            response.EnsureSuccessStatusCode(); // lan�a exce��o se n�o for sucesso

            var responseBody = await response.Content.ReadAsStringAsync(); // L� a resposta
            Console.WriteLine(responseBody); // Exibe o conte�do da resposta
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


    // Incrementando o contador de requisi��es para Prometheus
    requestCounter.Inc(); // Incrementa o contador para cada requisi��o recebida

    // Passando a requisi��o para o pr�ximo middleware
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