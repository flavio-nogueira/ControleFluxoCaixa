using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ControleFluxoCaixa.BFF.Dtos.Auth;
using ControleFluxoCaixa.BFF.Dtos.Lancamentos;
using ControleFluxoCaixa.BFF.Dtos.Lancamento;

public class BFFEndpointsFilter : IDocumentFilter
{
    /// <summary>
    /// Este filtro é aplicado ao Swagger para gerar endpoints manualmente no BFF.
    /// Ele remove caminhos indesejados e adiciona endpoints específicos do gateway,
    /// incluindo aqueles com parâmetros query (como paginação e datas).
    /// </summary>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // -------------------------------------------------------------
        // REMOVE caminhos indesejados da documentação (como config ou cache)
        // -------------------------------------------------------------
        var pathsParaRemover = swaggerDoc.Paths
            .Where(p => p.Key.StartsWith("/configuration") || p.Key.StartsWith("/outputcache"))
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsParaRemover)
            swaggerDoc.Paths.Remove(path);

        // -------------------------------------------------------------
        // DEFINIÇÃO MANUAL dos endpoints do BFF com:
        // (path, método HTTP, tag, descrição, tipo de request body, tipo de response)
        // -------------------------------------------------------------
        var endpoints = new List<(string path, OperationType method, string tag, string summary, Type? requestDto, Type? responseDto)>
        {
            // ----------------------- AUTH -----------------------
            ("/bff/auth/login",        OperationType.Post,   "Auth", "Login do usuário",           typeof(LoginDto),        typeof(RefreshDto)),
            ("/bff/auth/refresh",      OperationType.Post,   "Auth", "Atualizar token JWT",        typeof(RefreshDto),      typeof(RefreshDto)),
            ("/bff/auth/register",     OperationType.Post,   "Auth", "Registrar novo usuário",     typeof(RegisterDto),     typeof(UserDto)),
            ("/bff/auth",              OperationType.Put,    "Auth", "Atualizar dados do usuário", typeof(UpdateUserDto),   null),
            ("/bff/auth",              OperationType.Get,    "Auth", "Listar todos os usuários",   null,                    typeof(IEnumerable<UserDto>)),
            ("/bff/auth/{id}",         OperationType.Delete, "Auth", "Excluir usuário por ID",     null,                    null),
            ("/bff/auth/{id}",         OperationType.Get,    "Auth", "Obter usuário por ID",       null,                    typeof(UserDto)),

            // ------------------- LANCAMENTO (simples) -------------------
            ("/bff/lancamento/getall",             OperationType.Get,    "Lancamento", "Listar todos os lançamentos",             null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/getbytipo/{tipo}",   OperationType.Get,    "Lancamento", "Buscar lançamentos por tipo",            null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/getbyid/{id}",       OperationType.Get,    "Lancamento", "Buscar lançamento por ID",               null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/create",             OperationType.Post,   "Lancamento", "Criar novo lançamento",                  typeof(LancamentoDto), typeof(Guid)),
            ("/bff/lancamento/deletemany",         OperationType.Delete, "Lancamento", "Excluir múltiplos lançamentos",          typeof(List<Guid>), typeof(LancamentoResponseDto)),
            ("/bff/lancamento/saldos",             OperationType.Get,    "Lancamento", "Obter saldos no intervalo",              null, typeof(LancamentoResponseDto)),

            // ------------------- LANCAMENTO (paginado) -------------------
            ("/bff/lancamento/getallpaginado",            OperationType.Get, "Lancamento", "Listar lançamentos paginados",              null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/getbytipopaginado/{tipo}",  OperationType.Get, "Lancamento", "Listar lançamentos por tipo paginados",     null, typeof(LancamentoResponseDto)),
            ("/bff/lancamento/saldospaginado",            OperationType.Get, "Lancamento", "Listar saldos consolidados paginados",      null, typeof(LancamentoResponseDto)),
        };

        // -------------------------------------------------------------
        // PERCORRE cada endpoint definido acima para construir sua documentação
        // -------------------------------------------------------------
        foreach (var (path, method, tag, summary, requestDto, responseDto) in endpoints)
        {
            // Cria ou obtém o caminho da rota
            if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
            {
                pathItem = new OpenApiPathItem();
                swaggerDoc.Paths.Add(path, pathItem);
            }

            // Evita sobrescrever rota existente
            if (pathItem.Operations.ContainsKey(method))
                continue;

            // Criação da operação (endpoint) no Swagger
            var operation = new OpenApiOperation
            {
                Summary = summary,
                Tags = new List<OpenApiTag> { new() { Name = tag } },
                Responses = new OpenApiResponses()
            };

            // ---------------------------------------------------------
            // Caso o endpoint aceite um corpo (body), como POST/PUT/DELETE,
            // gera o schema para a requisição
            // ---------------------------------------------------------
            if (requestDto != null && (method == OperationType.Post || method == OperationType.Put || method == OperationType.Delete))
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

            // ---------------------------------------------------------
            // Adiciona parâmetros de paginação e data quando aplicável
            // ---------------------------------------------------------
            if (path.EndsWith("paginado"))
            {
                // Parâmetro page
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "page",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Número da página (padrão 1)",
                    Schema = new OpenApiSchema { Type = "integer", Default = new Microsoft.OpenApi.Any.OpenApiInteger(1) }
                });

                // Parâmetro pageSize
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "pageSize",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Itens por página (padrão 20)",
                    Schema = new OpenApiSchema { Type = "integer", Default = new Microsoft.OpenApi.Any.OpenApiInteger(20) }
                });

                // Parâmetros 'de' e 'ate' para saldos consolidados
                if (path.Contains("saldospaginado", StringComparison.OrdinalIgnoreCase))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "de",
                        In = ParameterLocation.Query,
                        Required = true,
                        Description = "Data inicial (formato yyyy-MM-dd)",
                        Schema = new OpenApiSchema { Type = "string", Format = "date" }
                    });

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "ate",
                        In = ParameterLocation.Query,
                        Required = true,
                        Description = "Data final (formato yyyy-MM-dd)",
                        Schema = new OpenApiSchema { Type = "string", Format = "date" }
                    });
                }
            }

            // ---------------------------------------------------------
            // Define a resposta 200 (sucesso) com o schema correspondente
            // ---------------------------------------------------------
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

            // Resposta genérica para erro interno
            operation.Responses["500"] = new OpenApiResponse { Description = "Erro interno" };

            // Associa a operação ao método HTTP correto
            pathItem.Operations[method] = operation;
        }
    }
}
