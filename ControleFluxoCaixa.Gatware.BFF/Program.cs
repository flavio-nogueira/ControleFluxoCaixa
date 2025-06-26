using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Arquivo de configuração Ocelot
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

//Controllers para Swagger e testes locais (se houver)
builder.Services.AddControllers();

// Ocelot
builder.Services.AddOcelot(builder.Configuration);

//Swagger + Suporte ao JWT + filtro opcional de endpoints manuais
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend For Frontend - BFF Api",
        Version = "v1",
        Description = "BFF para consumo seguro da API ControleFluxoCaixa",
        Contact = new OpenApiContact
        {
            Name = "Flavio Nogueira",
            Url = new Uri("https://startupinfosoftware.com.br"), 
            Email = "flavio@startupinfosoftware.com.br"  
        },
        License = new OpenApiLicense
        {
            Name = "Licença MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Suporte ao botão "Authorize"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Apenas se você estiver adicionando endpoints manualmente
    c.DocumentFilter<BFFEndpointsFilter>();
});

// Prometheus
var requestCounter = Metrics.CreateCounter("api_requests_total", "Contador de requisições");

var app = builder.Build();

// Middleware Prometheus
app.Use(async (context, next) =>
{
    requestCounter.Inc();
    await next();
});
app.MapMetrics();

// Middleware Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BFF API v1");
});

// Mapear controllers locais
app.MapControllers();

// Ocelot como última etapa
await app.UseOcelot();

app.Run();
