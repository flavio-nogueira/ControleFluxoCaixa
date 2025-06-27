using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace ControleFluxoCaixa.Tests.Integration.Lancamentos
{
    public class LancamentoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LancamentoIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Deve_retornar_erro_quando_descricao_estiver_vazia()
        {
            var comando = new CreateLancamentoCommand
            {
                Itens = new List<Itens>
        {
            new Itens
            {
                Data = DateTime.UtcNow,
                Valor = 100.0m,
                Descricao = "", // Inválido
                Tipo = TipoLancamento.Credito
            }
        }
            };

            var response = await _client.PostAsJsonAsync("/api/lancamento/create", comando);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        //[Fact]
        //public async Task Deve_retornar_lista_vazia_por_tipo_quando_nao_existem_registros()
        //{
        //    var response = await _client.GetAsync("/api/lancamento/GetByTipo/0");
        //    response.EnsureSuccessStatusCode();

        //    var resultado = await response.Content.ReadFromJsonAsync<LancamentoResponseDto>();
        //    Assert.True(resultado!.Sucesso);
        //    Assert.Equal(0, resultado.Registros);
        //}

        //[Fact]
        //public async Task Deve_retornar_lista_total_vazia_quando_nao_ha_lancamentos()
        //{
        //    var response = await _client.GetAsync("/api/lancamento/GetAll");
        //    response.EnsureSuccessStatusCode();

        //    var resultado = await response.Content.ReadFromJsonAsync<LancamentoResponseDto>();
        //    Assert.True(resultado!.Sucesso);
        //    Assert.Equal(0, resultado.Registros);
        //}

        //[Fact]
        //public async Task Deve_retornar_erro_ao_excluir_lista_vazia()
        //{
        //    var response = await _client.DeleteAsync("/api/lancamento/DeleteMany");
        //    Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        //}

        //[Fact]
        //public async Task Deve_criar_lancamento_com_sucesso()
        //{
        //    var comando = new CreateLancamentoCommand
        //    {
        //        Itens = new List<Itens>
        //        {
        //            new()
        //            {
        //                Data = DateTime.UtcNow,
        //                Valor = 100.50m,
        //                Descricao = "Teste Integração",
        //                Tipo = TipoLancamento.Credito
        //            }
        //        }
        //    };

        //    var response = await _client.PostAsJsonAsync("/api/lancamento/create", comando);

        //    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        //    var id = await response.Content.ReadFromJsonAsync<Guid>();
        //    Assert.NotEqual(Guid.Empty, id);
        //}
    }
}
