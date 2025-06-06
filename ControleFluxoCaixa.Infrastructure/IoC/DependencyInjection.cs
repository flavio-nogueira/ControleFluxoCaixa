﻿using ControleFluxoCaixa.Application.Commands.Auth.DeleteUser;
using ControleFluxoCaixa.Application.Commands.Auth.Login;
using ControleFluxoCaixa.Application.Commands.Auth.RegisterUser;
using ControleFluxoCaixa.Application.Commands.Auth.UpdateUser;
using ControleFluxoCaixa.Application.Interfaces.Cache;
using ControleFluxoCaixa.Application.Interfaces.Seed;
using ControleFluxoCaixa.Application.Queries.Auth.GetAllUsers;
using ControleFluxoCaixa.Application.Queries.Auth.GetUserById;
using ControleFluxoCaixa.Application.Services;
using ControleFluxoCaixa.Infrastructure.IoC.Auth;
using ControleFluxoCaixa.Infrastructure.IoC.DataBase;
using ControleFluxoCaixa.Infrastructure.IoC.Jwt;
using ControleFluxoCaixa.Infrastructure.IoC.Swagger;
using ControleFluxoCaixa.Infrastructure.Seeders;
using ControleFluxoCaixa.Infrastructure.Services.Seed;
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
            services.AddControllers();

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
            services.AddDistributedMemoryCache();

            // Registro do serviço de cache genérico baseado em Redis
            services.AddScoped<IGenericCacheService, GenericCacheService>();

            // Registra todos os command/handlers da aplicação com MediatR
            services.AddMediatR(
                typeof(LoginCommand).Assembly,
                typeof(RegisterUserCommand).Assembly,
                typeof(UpdateUserCommand).Assembly,
                typeof(DeleteUserCommand).Assembly,
                typeof(GetAllUsersQuery).Assembly,
                typeof(GetUserByIdQuery).Assembly
            );

            // Registra o serviço responsável por executar o seed do usuário Admin        
            services.AddScoped<SeedIdentityAdminUser>();

            // Serviço responsável por registrar e verificar a execução de seeds (ex: garantir que cada seed rode só uma vez)
            services.AddScoped<ISeederService, SeederService>();

            return services;
        }
    }
}
