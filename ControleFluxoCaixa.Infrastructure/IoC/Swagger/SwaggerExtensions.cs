using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;

namespace ControleFluxoCaixa.Infrastructure.IoC.Swagger
{
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Registra o SwaggerGen e configura o documento OpenAPI v1,
        /// além de adicionar a segurança Bearer JWT e incluir comentários XML.
        /// Ajustado para compatibilidade com Swagger 2.0 (SerializeAsV2), usando ApiKey.
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {


            // Necessário para Swagger funcionar no .NET 6/7/8
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {             
                // Definição do documento OpenAPI/Swagger
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ControleFluxoCaixa API",
                    Version = "v1",
                    Description = "API para controle de fluxo de caixa",
                    Contact = new OpenApiContact
                    {
                        Name = "Flavio Nogueira",
                        Email = "flavio@startupinfosoftware.com.br",
                        Url = new Uri("tel:+5511999999999") // Nota: URI tipo "tel:" é válido, mas raramente usado na seção de contato
                    }
                });

                // XML comments (opcional)
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Segurança: ApiKey no header para compatibilidade com Swagger 2.0
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT no cabeçalho: Bearer {seu_token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer" // Essencial para que a interface Swagger reconheça o nome do esquema
                });

                // Aplica o esquema de segurança globalmente
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            In = ParameterLocation.Header,
                            Name = "Authorization",
                            Scheme = "Bearer"
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
