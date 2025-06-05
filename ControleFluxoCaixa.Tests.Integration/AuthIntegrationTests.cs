using ControleFluxoCaixa.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ControleFluxoCaixa.Tests.Integration.Identity
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Deve_registrar_e_autenticar_usuario_com_sucesso()
        {
            // Este teste verifica se o registro e autenticação de um usuário funcionam corretamente.
            // Primeiro, registra o usuário via /api/auth/register. Em seguida, realiza o login via /api/auth/login.
            // Verifica se o login retorna 200 OK e se os tokens JWT e Refresh Token são válidos.

            var register = new RegisterDto { Email = "teste@teste.com", Password = "Senha123!", FullName = "Usuário Teste" };
            var login = new LoginDto { Email = register.Email, Password = register.Password };

            await _client.PostAsJsonAsync("/api/auth/register", register);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var content = await loginResponse.Content.ReadFromJsonAsync<RefreshDto>();
            Assert.False(string.IsNullOrWhiteSpace(content?.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(content?.RefreshToken));
        }

        [Fact]
        public async Task Deve_renovar_token_com_refresh_token_valido()
        {
            // Este teste garante que um token de acesso possa ser renovado com um refresh token válido.
            // Após registrar e autenticar o usuário, é feito um POST em /api/auth/refresh com o refresh token.
            // O teste valida que o novo token é diferente do anterior e que a resposta é 200 OK.

            var register = new RegisterDto { Email = "refresh@teste.com", Password = "Senha123!", FullName = "Usuário Refresh" };
            var login = new LoginDto { Email = register.Email, Password = register.Password };

            await _client.PostAsJsonAsync("/api/auth/register", register);
            var loginResult = await _client.PostAsJsonAsync("/api/auth/login", login);
            var tokens = await loginResult.Content.ReadFromJsonAsync<RefreshDto>();

            var refreshDto = new RefreshDto
            {
                AccessToken = tokens!.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt
            };

            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

            var newTokens = await refreshResponse.Content.ReadFromJsonAsync<RefreshDto>();
            Assert.NotEqual(tokens.AccessToken, newTokens?.AccessToken);
        }

        [Fact]
        public async Task Nao_deve_renovar_token_com_refresh_token_invalido()
        {
            // Testa se o sistema rejeita a renovação de token com um refresh token inválido.
            // Envia um DTO com tokens falsos e espera resposta 401 Unauthorized.

            var refreshDto = new RefreshDto
            {
                AccessToken = "token_falso",
                RefreshToken = "refresh_token_falso",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Nao_deve_acessar_endpoint_protegido_sem_token()
        {
            // Verifica se o acesso a um endpoint protegido sem token JWT retorna 401 Unauthorized.

            var updateDto = new UpdateUserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "falso@teste.com"
            };

            var response = await _client.PutAsJsonAsync("/api/auth", updateDto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Nao_deve_acessar_com_token_invalido()
        {
            // Testa se um token JWT inválido é corretamente rejeitado ao tentar acessar endpoint protegido.

            var updateDto = new UpdateUserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "falso@teste.com"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token_invalido");
            request.Content = JsonContent.Create(updateDto);

            var response = await _client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Deve_acessar_endpoint_protegido_com_token_valido()
        {
            // Garante que um token JWT válido permite o acesso ao endpoint protegido.
            // Após registrar e autenticar, realiza uma requisição PUT autorizada com token válido.

            var register = new RegisterDto { Email = "tokenvalido@teste.com", Password = "Senha123!", FullName = "Token Válido" };
            var login = new LoginDto { Email = register.Email, Password = register.Password };

            await _client.PostAsJsonAsync("/api/auth/register", register);
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);
            var content = await loginResponse.Content.ReadFromJsonAsync<RefreshDto>();

            var updateDto = new UpdateUserDto
            {
                Id = "não_precisa_existir",
                Email = register.Email,
                FullName = "Atualizado"
            };

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", content!.AccessToken);
            request.Content = JsonContent.Create(updateDto);

            var response = await _client.SendAsync(request);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Nao_deve_autenticar_com_senha_errada()
        {
            // Testa se o login falha quando a senha informada está incorreta.
            // Espera resposta 401 Unauthorized do endpoint /api/auth/login.

            var register = new RegisterDto { Email = "senhaerrada@teste.com", Password = "Senha123!", FullName = "Erro de Senha" };
            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginDto { Email = register.Email, Password = "senhaErrada" };
            var loginResult = await _client.PostAsJsonAsync("/api/auth/login", login);

            Assert.Equal(HttpStatusCode.Unauthorized, loginResult.StatusCode);
        }
    }
}
