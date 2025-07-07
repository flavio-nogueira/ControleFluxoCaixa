
// Versão com CQRS, Logs estruturados (Serilog), Métricas Prometheus e Polly.
// Todos os pontos de falha agora registram entradas no Loki.

using ControleFluxoCaixa.Application.Commands.Auth.DeleteUser;
using ControleFluxoCaixa.Application.Commands.Auth.Login;
using ControleFluxoCaixa.Application.Commands.Auth.RefreshToken;
using ControleFluxoCaixa.Application.Commands.Auth.RegisterUser;
using ControleFluxoCaixa.Application.Commands.Auth.UpdateUser;
using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using Prometheus;

namespace ControleFluxoCaixa.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        // ────── Métricas Prometheus ────────────────────────────────────────────
        private static readonly Counter LoginCounter =
            Metrics.CreateCounter("auth_login_requests_total", "Total de requisições de login");

        private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
            "auth_request_duration_seconds",
            "Duração das requisições de autenticação",
            new HistogramConfiguration { LabelNames = new[] { "method", "endpoint", "status" } });

        // ────── Politica de Retry (Polly) ──────────────────────────────────────
        private static readonly AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>Realiza login e retorna JWT + refresh-token.</summary>
        [HttpPost("login"), AllowAnonymous]
        [ProducesResponseType(typeof(RefreshDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("POST", "login", "").NewTimer();
            LoginCounter.Inc();

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _mediator.Send(new LoginCommand(dto.Email, dto.Password, ip), cancellationToken);

                timer.ObserveDuration(); // status 200
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) // ← captura a exceção
            {
                timer.ObserveDuration();            // status 401
                _logger.LogWarning(ex,
                    "Tentativa de login inválida para o e-mail {Email}.", dto.Email);

                return Unauthorized("Credenciais inválidas.");
            }
            catch (Exception ex)
            {
                timer.ObserveDuration();            // status 500
                _logger.LogError(ex,
                    "Erro interno ao tentar logar com o e-mail {Email}.", dto.Email);

                return StatusCode(500, "Erro interno no login.");
            }
        }

        /// <summary>Renova o JWT com um refresh-token válido.</summary>
        [HttpPost("refresh"), AllowAnonymous]
        [ProducesResponseType(typeof(RefreshDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto, CancellationToken cancellationToken)
        {
            var timer = RequestDuration.WithLabels("POST", "refresh", "").NewTimer();

            try
            {
                var result = await _mediator.Send(new RefreshTokenCommand(dto.RefreshToken), cancellationToken);

                timer.ObserveDuration(); // status 200
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                timer.ObserveDuration(); // status 401
                _logger.LogWarning(ex, "Refresh-token inválido ou já utilizado.");
                return Unauthorized("Token inválido ou já utilizado.");
            }
            catch (Exception ex)
            {
                timer.ObserveDuration(); // status 500
                _logger.LogError(ex, "Erro ao renovar token.");
                return StatusCode(500, "Erro interno ao renovar token.");
            }
        }

        /// <summary>Registra um novo usuário.</summary>
        [HttpPost("register"), AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(
                    new RegisterUserCommand(dto.Email, dto.Password, dto.FullName), cancellationToken);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "Tentativa de registro com e-mail já existente: {Email}.", dto.Email);

                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao registrar usuário {Email}.", dto.Email);

                return StatusCode(500, "Erro interno ao registrar usuário.");
            }
        }

        /// <summary>Atualiza dados de um usuário e/ou redefine a senha.</summary>
        [HttpPut, Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(
                    new UpdateUserCommand(dto.Id, dto.Email, dto.FullName, dto.NewPassword), cancellationToken);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "Usuário {UserId} não encontrado para atualização.", dto.Id);

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar usuário {UserId}.", dto.Id);

                return StatusCode(500, "Erro interno ao atualizar usuário.");
            }
        }

        /// <summary>Remove um usuário existente.</summary>
        [HttpDelete("{id}"), Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "Usuário {UserId} não encontrado para exclusão.", id);

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao excluir usuário {UserId}.", id);

                return StatusCode(500, "Erro interno ao excluir usuário.");
            }
        }

        /// <summary>Retorna todos os usuários.</summary>
        [HttpGet, Authorize]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await RetryPolicy.ExecuteAsync(
                    () => _mediator.Send(new GetAllUsersQuery(), cancellationToken));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todos os usuários.");
                return StatusCode(500, "Erro interno ao recuperar usuários.");
            }
        }

        /// <summary>Retorna dados do usuário pelo ID.</summary>
        [HttpGet("{id}"), Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao recuperar usuário {UserId}.", id);

                return StatusCode(500, "Erro interno ao recuperar usuário.");
            }
        }
    }
}
