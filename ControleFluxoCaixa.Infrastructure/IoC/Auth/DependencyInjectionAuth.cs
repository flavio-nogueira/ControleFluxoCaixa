using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Domain.Entities.User;
using ControleFluxoCaixa.Infrastructure.Context.Identity;
using ControleFluxoCaixa.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFluxoCaixa.Infrastructure.IoC.Auth
{
    public static class DependencyInjectionAuth

    {
        // Serviços de autenticação

        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {

            // 3) Configuração do ASP.NET Core Identity   
            // Adiciona Identity, definindo ApplicationUser e IdentityRole<Guid> como entidades de usuário e papel (role).
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opts =>
            {
                // Configurações de política de senha:
                opts.Password.RequireDigit = true;             // Exige ao menos dígito numérico
                opts.Password.RequireLowercase = true;         // Exige ao menos letra minúscula
                opts.Password.RequireNonAlphanumeric = false;  // Não exige caracteres especiais
                opts.Password.RequiredLength = 6;              // Comprimento mínimo de 6 caracteres
            })
                // Informa que o Identity usará o IdentityContext para persistir usuários e papéis
                .AddEntityFrameworkStores<IdentityDBContext>()
                // Adiciona provedores de token (para recuperação de senha, confirmação de e-mail, etc.)
                .AddDefaultTokenProviders();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            return services;
        }
    }
}
