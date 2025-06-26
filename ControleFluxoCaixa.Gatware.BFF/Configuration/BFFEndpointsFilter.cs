using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class BFFEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove rotas indesejadas
        var pathsParaRemover = swaggerDoc.Paths
            .Where(p => p.Key.StartsWith("/configuration") || p.Key.StartsWith("/outputcache"))
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsParaRemover)
        {
            swaggerDoc.Paths.Remove(path);
        }

        // Endpoints a adicionar manualmente
        var endpoints = new List<(string path, OperationType method, string tag, string summary)>
        {
            ("/bff/auth/login", OperationType.Post, "Auth", "Login do usuário"),
            ("/bff/auth/refresh", OperationType.Post, "Auth", "Atualizar token JWT"),
            ("/bff/auth/register", OperationType.Post, "Auth", "Registrar novo usuário"),
            ("/bff/auth", OperationType.Put, "Auth", "Atualizar dados do usuário"),
            ("/bff/auth", OperationType.Get, "Auth", "Listar todos os usuários"),
            ("/bff/auth/{id}", OperationType.Delete, "Auth", "Excluir usuário por ID"),
            ("/bff/auth/{id}", OperationType.Get, "Auth", "Obter usuário por ID"),

            ("/bff/lancamento/getall", OperationType.Get, "Lancamento", "Listar todos os lançamentos"),
            ("/bff/lancamento/getbytipo/{tipo}", OperationType.Get, "Lancamento", "Buscar lançamentos por tipo"),
            ("/bff/lancamento/getbyid/{id}", OperationType.Get, "Lancamento", "Buscar lançamento por ID"),
            ("/bff/lancamento/create", OperationType.Post, "Lancamento", "Criar novo lançamento"),
            ("/bff/lancamento/deletemany", OperationType.Delete, "Lancamento", "Excluir múltiplos lançamentos"),
            ("/bff/lancamento/saldos", OperationType.Get, "Lancamento", "Obter saldos no intervalo")
        };

        // Adiciona cada operação ao path correspondente
        foreach (var (path, method, tag, summary) in endpoints)
        {
            if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
            {
                pathItem = new OpenApiPathItem();
                swaggerDoc.Paths.Add(path, pathItem);
            }

            // Evita sobrescrever se já houver uma operação do mesmo tipo
            if (!pathItem.Operations.ContainsKey(method))
            {
                pathItem.Operations[method] = new OpenApiOperation
                {
                    Summary = summary,
                    Tags = new List<OpenApiTag> { new() { Name = tag } },
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Sucesso" },
                        ["500"] = new OpenApiResponse { Description = "Erro interno" }
                    }
                };
            }
        }
    }
}
