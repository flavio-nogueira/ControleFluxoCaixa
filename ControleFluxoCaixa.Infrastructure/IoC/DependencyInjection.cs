using ControleFluxoCaixa.Application.Filters;
using ControleFluxoCaixa.Application.Handlers;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Application.Interfaces.Seed;
using ControleFluxoCaixa.Application.Pipelines;
using ControleFluxoCaixa.Application.Queries;
using ControleFluxoCaixa.Application.Validators.Lancamento;
using ControleFluxoCaixa.CrossCutting.Mapping;
using ControleFluxoCaixa.Domain.Interfaces;
using ControleFluxoCaixa.Infrastructure.Cache;
using ControleFluxoCaixa.Infrastructure.IoC.Auth;
using ControleFluxoCaixa.Infrastructure.IoC.DataBase;
using ControleFluxoCaixa.Infrastructure.IoC.Jwt;
using ControleFluxoCaixa.Infrastructure.IoC.MongoDB;
using ControleFluxoCaixa.Infrastructure.IoC.Swagger;
using ControleFluxoCaixa.Infrastructure.Repositories;
using ControleFluxoCaixa.Infrastructure.Seeders;
using ControleFluxoCaixa.Infrastructure.Services.Seed;
using ControleFluxoCaixa.Messaging.MessagingSettings;
using ControleFluxoCaixa.Messaging.Publishers;
using ControleFluxoCaixa.Mongo.Repositories;
using ControleFluxoCaixa.MongoDB.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ControleFluxoCaixa.Infrastructure.IoC
{
    /// <summary>
    /// Classe responsável por registrar todos os serviços e dependências da aplicação,
    /// centralizando os pontos de injeção para manter o Program.cs limpo e organizado.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra todos os serviços da aplicação no container de injeção de dependência (DI).
        /// </summary>
        /// <param name="services">Coleção de serviços do ASP.NET Core</param>
        /// <param name="configuration">Configurações da aplicação (appsettings.json)</param>
        /// <returns>Serviços registrados (encadeamento)</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Adiciona os serviços de controller necessários para a aplicação funcionar 
            services.AddControllers(options =>
            {
                options.Filters.Add<ModelStateValidationFilter>();
            });

            // Registra AutoMapper 

            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Registra o HttpClient nomeado "ApiInterna" com políticas resilientes (Timeout, Retry e CircuitBreaker via Polly)
            services.AddResilientHttpClient();

            // Infraestrutura de banco de dados (DbContexts, repositórios, migrations)
            services.AddInfrastructure(configuration);

            // Serviços de autenticação e identidade (UserManager, TokenService, etc)
            services.AddAuthServices();

            // Configuração do JWT Bearer Token
            services.AddJwtAuthentication(configuration);

            // Configuração da documentação Swagger/OpenAPI
            services.AddSwaggerDocumentation();

            // Cache na memória (opcional, mas usado por tokens ou serviços temporários)
            services.AddMemoryCache();

            // Registro do serviço de cache genérico baseado em Redis
            services.AddScoped<ICacheService, CacheService>();

            // Registra o repositório de lançamentos.
            // A interface ILancamentoRepository será resolvida para a implementação LancamentoRepository
            // sempre que for injetada em construtores.
            services.AddScoped<ILancamentoRepository, LancamentoRepository>();

            // Registra o publicador genérico de mensagens RabbitMQ.
            // Permite injetar IRabbitMqPublisher<T> e receber RabbitMqPublisher<T>, com T sendo o tipo da mensagem.
            // Ex: IRabbitMqPublisher<LancamentoDto>
            services.AddScoped(typeof(IRabbitMqPublisher<>), typeof(RabbitMqPublisher<>));

            // Mapeia a seção "RabbitMqSettings" do appsettings.json para a classe fortemente tipada RabbitMqSettings.
            // Assim, a classe RabbitMqPublisher<T> pode acessar as configurações de URI, fila e exchange via IOptions<RabbitMqSettings>.
            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));

            // MongoDB
            services.AddMongoDb(configuration);

            // Registra todos os command/handlers da aplicação com MediatR
            services.AddMediatR(
                typeof(CreateLancamentoCommandHandler).Assembly,
                typeof(DeleteLancamentoCommandHandler).Assembly,
                typeof(GetAllLancamentosQueryHandler).Assembly,
                typeof(GetAllUsersQueryHandler).Assembly,
                typeof(GetUserByIdQueryHandler).Assembly,
                typeof(ListLancamentosQueryHandler).Assembly,
                typeof(GetSaldosConsolidadosQueryHandler).Assembly
                

            );

            services.AddValidatorsFromAssemblyContaining<CreateLancamentoCommandValidator>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Registra o serviço responsável por executar o seed do usuário Admin        
            services.AddScoped<SeedIdentityAdminUser>();

            // Serviço responsável por registrar e verificar a execução de seeds (ex: garantir que cada seed rode só uma vez)
            services.AddScoped<ISeederService, SeederService>();

            services.AddScoped<ISaldoDiarioConsolidadoRepository, SaldoDiarioConsolidadoRepository>();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy", builder =>
            //    {
            //        builder
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader();
            //    });
            //});


            return services;
        }
    }
}
