using Microsoft.Extensions.DependencyInjection;
using Polly; // Núcleo do Polly para políticas de resiliência
using Polly.Extensions.Http; // Extensões específicas para uso com HttpClient
using System.Net; // Para trabalhar com códigos de status HTTP

namespace ControleFluxoCaixa.Infrastructure.IoC
{
    /// <summary>
    /// Extensões para registrar políticas de resiliência com Polly no HttpClient.
    /// </summary>
    public static class PollyPoliciesExtensions
    {
        /// <summary>
        /// Registra um HttpClient com políticas de timeout, retry e circuit breaker.
        /// Utiliza a porta HTTP (5000) ou HTTPS (5001), dependendo do ambiente.
        /// </summary>
        /// <param name="services">Coleção de serviços para DI.</param>
        public static IServiceCollection AddResilientHttpClient(this IServiceCollection services)
        {
            var baseUrl = Environment.GetEnvironmentVariable("USE_HTTPS") == "true"
                ? "https://localhost:5001"
                : "http://localhost:5000";

            services.AddHttpClient("ApiInterna", client =>
            {
                client.BaseAddress = new Uri(baseUrl); // URL da API, baseada na variável de ambiente
            })
            .AddPolicyHandler(GetTimeoutPolicy())          // Aplica timeout de 10s
            .AddPolicyHandler(GetRetryPolicy())            // Aplica 3 tentativas com backoff exponencial
            .AddPolicyHandler(GetCircuitBreakerPolicy());  // Aplica Circuit Breaker para falhas consecutivas

            return services;
        }

        /// <summary>
        /// Política de retry com backoff exponencial.
        /// É acionada em erros transitórios ou status 429 (Too Many Requests).
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError() // Captura erros 5xx, 408, etc.
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // Ou se for 429
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Espera 2s, 4s, 8s

        /// <summary>
        /// Política de Circuit Breaker: "abre" o circuito após 5 falhas consecutivas.
        /// Mantém o circuito aberto por 30 segundos antes de tentar novamente.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));

        /// <summary>
        /// Política de timeout: cancela requisições que demorarem mais de 10 segundos.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
            Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
    }
}