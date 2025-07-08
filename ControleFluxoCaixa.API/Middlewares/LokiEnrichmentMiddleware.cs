// -----------------------------------------------------------------------------
// LokiEnrichmentMiddleware.cs
// Enriquecedor de logs para toda requisição HTTP.
// Adiciona labels (endpoint, method, user, status) que o sink Loki converterá
// em labels de consulta no Grafana, melhorando rastreabilidade e análise.
// -----------------------------------------------------------------------------

using Serilog;
using Serilog.Context; //Permite adicionar propriedades customizadas ao contexto dos logs (LogContext)
using System.Security.Claims;

namespace ControleFluxoCaixa.API.Middlewares
{
    /// <summary>
    /// Middleware responsável por enriquecer os logs com informações relevantes
    /// como endpoint acessado, método HTTP, usuário autenticado e status da resposta.
    /// </summary>
    public class LokiEnrichmentMiddleware
    {
        private readonly RequestDelegate _next; // Delegado para continuar a execução da pipeline

        public LokiEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Intercepta cada requisição HTTP e injeta informações úteis no contexto de log.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Coleta de informações da requisição ---------
            var endpoint = context.Request.Path.Value?.Trim('/').ToLower() ?? "unknown"; // Exemplo: api/auth/login
            var method = context.Request.Method; // GET, POST, PUT, DELETE...
            var user = context.User?.Identity?.IsAuthenticated == true
                ? context.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown" // Se autenticado, tenta pegar o e-mail
                : "anonymous"; // Caso contrário, marca como "anonymous"

            // Enriquecimento do LogContext com endpoint, método e usuário ---------
            using (LogContext.PushProperty("endpoint", endpoint))
            using (LogContext.PushProperty("method", method))
            using (LogContext.PushProperty("user", user))
            {
                // Redireciona a resposta para um MemoryStream temporário ---------
                var originalBody = context.Response.Body;         // Armazena o body original da resposta
                using var memoryStream = new MemoryStream();       // Cria um novo stream temporário
                context.Response.Body = memoryStream;              // Substitui pelo stream temporário

                // Executa o próximo middleware/controller na pipeline ---------
                await _next(context); // Aqui a requisição "segue o fluxo normal"

                // Restaura o body original e lê o status da resposta ---------
                context.Response.Body.Seek(0, SeekOrigin.Begin);   // Volta para o início do stream
                await memoryStream.CopyToAsync(originalBody);      // Copia o conteúdo de volta ao body original

                var statusCode = context.Response.StatusCode;      // Captura o código de status HTTP (200, 404, 500...)

                //Adiciona statusCode ao contexto do log ---------
                using (LogContext.PushProperty("status", statusCode))
                {
                    // Log final da requisição com status já incluído como label no Loki
                    Log.Information("Request finalizada com status {status}", statusCode);
                }
            }
        }
    }
}
