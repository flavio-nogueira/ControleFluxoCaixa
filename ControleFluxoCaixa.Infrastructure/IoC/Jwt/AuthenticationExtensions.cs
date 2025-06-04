using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ControleFluxoCaixa.Infrastructure.IoC.Jwt
{
    /// <summary>
    /// Classe estática para configurar a autenticação JWT no pipeline de serviços.
    /// Contém um método de extensão para registrar a autenticação JWT no IServiceCollection.
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adiciona a configuração de autenticação JWT (Bearer) ao contêiner de serviços.
        /// </summary>
        /// <param name="services">Coleção de serviços do ASP.NET Core.</param>
        /// <param name="config">Configuração geral da aplicação (appsettings.json, variáveis de ambiente etc.).</param>
        /// <returns>IServiceCollection com autenticação e autorização registrados.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Recupera a chave secreta (Key), emissor (Issuer) e público-alvo (Audience) definidos no appsettings.json
            var key = config["JwtSettings:SecretKey"]!;
            var issuer = config["JwtSettings:Issuer"]!;
            var audience = config["JwtSettings:Audience"]!;

            // Registra o serviço de autenticação e especifica o esquema padrão como JWT Bearer
            services
                .AddAuthentication(options =>
                {
                    // Define o esquema de autenticação padrão para validar tokens JWT
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    // Define o esquema para desafiar (challenge) quando necessário
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Não exige metadata via HTTPS (útil para ambientes de desenvolvimento)
                    options.RequireHttpsMetadata = false;
                    // Guarda o token no contexto de autenticação (para usos posteriores, se necessário)
                    options.SaveToken = true;

                    // Parâmetros para validação do token JWT
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Valida se o emissor do token é o esperado
                        ValidateIssuer = true,
                        // Valida se o público-alvo do token é o esperado
                        ValidateAudience = true,
                        // Valida se o token não está expirado
                        ValidateLifetime = true,
                        // Valida a assinatura do emissor
                        ValidateIssuerSigningKey = true,
                        // Define o emissor válido (Issuer)
                        ValidIssuer = issuer,
                        // Define o público-alvo válido (Audience)
                        ValidAudience = audience,
                        // Converte a chave secreta em bytes e define a chave de assinatura simétrica
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(key))
                    };

                    // Eventos que podem ser interceptados durante o processo de autenticação
                    options.Events = new JwtBearerEvents
                    {
                        // Executado quando a validação do token falha
                        OnAuthenticationFailed = ctx =>
                        {
                            // Exibe no console o motivo da falha de validação JWT
                            Console.Error.WriteLine($"JWT Validation Failed: {ctx.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        // Executado quando o token é validado com sucesso
                        OnTokenValidated = ctx =>
                        {
                            // Exibe no console o nome (Identity.Name) do usuário autenticado
                            Console.WriteLine("JWT Validated for " + ctx.Principal?.Identity?.Name);
                            return Task.CompletedTask;
                        }
                    };
                });

            // Adiciona serviços de autorização (políticas, roles etc.)
            services.AddAuthorization();

            return services;
        }
    }
}
// Este código configura a autenticação JWT no ASP.NET Core, permitindo que a aplicação valide tokens JWT
