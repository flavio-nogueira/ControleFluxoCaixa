
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ControleFluxoCaixa.BFF.Dtos.Auth;
using ControleFluxoCaixa.BFF.Dtos.Lancamentos;
using ControleFluxoCaixa.BFF.Dtos.Lancamento;

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

        // Endpoints mapeados manualmente com suporte a schemas
        var endpoints = new List<(string path, OperationType method, string tag, string summary, Type? requestDto, Type? responseDto)>
        {
            ("/bff/auth/login", OperationType.Post, "Auth", "Login do usuário", typeof(LoginDto), typeof(RefreshDto)),
            ("/bff/auth/refresh", OperationType.Post, "Auth", "Atualizar token JWT", typeof(RefreshDto), typeof(RefreshDto)),
            ("/bff/auth/register", OperationType.Post, "Auth", "Registrar novo usuário", typeof(RegisterDto), typeof(UserDto)),
            ("/bff/auth", OperationType.Put, "Auth", "Atualizar dados do usuário", typeof(UpdateUserDto), null),
            ("/bff/auth", OperationType.Get, "Auth", "Listar todos os usuários", null, typeof(IEnumerable<UserDto>)),
            ("/bff/auth/{id}", OperationType.Delete, "Auth", "Excluir usuário por ID", null, null),
            ("/bff/auth/{id}", OperationType.Get, "Auth", "Obter usuário por ID", null, typeof(UserDto)),

            ("/bff/lancamento/getall", OperationType.Get, "Lancamento", "Listar todos os lançamentos", null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/getbytipo/{tipo}", OperationType.Get, "Lancamento", "Buscar lançamentos por tipo", null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/getbyid/{id}", OperationType.Get, "Lancamento", "Buscar lançamento por ID", null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/create", OperationType.Post, "Lancamento", "Criar novo lançamento", typeof(LancamentoDto), typeof(Guid)),
            ("/bff/lancamento/deletemany", OperationType.Delete, "Lancamento", "Excluir múltiplos lançamentos", typeof(List<Guid>), typeof(LancamentoResponseDto)),
            ("/bff/lancamento/saldos", OperationType.Get, "Lancamento", "Obter saldos no intervalo", null, typeof(LancamentoResponseDto))
        };

        foreach (var (path, method, tag, summary, requestDto, responseDto) in endpoints)
        {
            if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
            {
                pathItem = new OpenApiPathItem();
                swaggerDoc.Paths.Add(path, pathItem);
            }

            if (!pathItem.Operations.ContainsKey(method))
            {
                var operation = new OpenApiOperation
                {
                    Summary = summary,
                    Tags = new List<OpenApiTag> { new() { Name = tag } },
                    Responses = new OpenApiResponses()
                };

                if (requestDto != null)
                {
                    var requestSchema = context.SchemaGenerator.GenerateSchema(requestDto, context.SchemaRepository);
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Required = true,
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType { Schema = requestSchema }
                        }
                    };
                }

                if (responseDto != null)
                {
                    var responseSchema = context.SchemaGenerator.GenerateSchema(responseDto, context.SchemaRepository);
                    operation.Responses["200"] = new OpenApiResponse
                    {
                        Description = "Sucesso",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType { Schema = responseSchema }
                        }
                    };
                }
                else
                {
                    operation.Responses["200"] = new OpenApiResponse { Description = "Sucesso" };
                }

                operation.Responses["500"] = new OpenApiResponse { Description = "Erro interno" };

                pathItem.Operations[method] = operation;
            }
        }
    }
}
