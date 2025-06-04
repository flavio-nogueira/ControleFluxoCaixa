using ControleFluxoCaixa.Application.DTOs;
using ControleFluxoCaixa.Application.DTOs.Auth;
using ControleFluxoCaixa.Application.Interfaces.Auth;
using ControleFluxoCaixa.Domain.Entities.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ControleFluxoCaixa.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenSvc;
        private readonly IRefreshTokenService _rtSvc;
        private readonly IDistributedCache _cache;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenSvc,
            IRefreshTokenService rtSvc,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _tokenSvc = tokenSvc;
            _rtSvc = rtSvc;
            _cache = cache;
        }

        /// <summary>
        /// Faz login e retorna um JWT + refresh token.
        /// </summary>
        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            var jwt = await _tokenSvc.GenerateAccessTokenAsync(user);
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(
                              user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            // Contador de logins bem-sucedidos
            var loginKey = $"logins:{user.Id}";
            var count = int.TryParse(await _cache.GetStringAsync(loginKey), out var c) ? c + 1 : 1;
            await _cache.SetStringAsync(loginKey, count.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            return Ok(new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            });
        }

        /// <summary>
        /// Renova o JWT usando um refresh token válido.
        /// </summary>
        [HttpPost("refresh"), AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshDto dto)
        {
            var blKey = $"rt_blacklist:{dto.RefreshToken}";
            if (await _cache.GetStringAsync(blKey) != null)
                return Unauthorized("Refresh token já consumido.");

            var validation = await _rtSvc.ValidateAndConsumeRefreshTokenAsync(dto.RefreshToken);
            if (!validation.IsValid || validation.User == null)
                return Unauthorized(new { message = "RefreshToken inválido ou expirado." });

            // Marca como consumido
            await _cache.SetStringAsync(blKey, "used", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });

            var jwt = await _tokenSvc.GenerateAccessTokenAsync(validation.User);
            var refresh = await _rtSvc.GenerateRefreshTokenAsync(
                              validation.User, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            return Ok(new RefreshDto
            {
                AccessToken = jwt,
                RefreshToken = refresh.Token,
                ExpiresAt = refresh.Expires
            });
        }

        /// <summary>
        /// Registra um novo usuário.
        /// </summary>
        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Conflict("E-mail já cadastrado.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };
            var res = await _userManager.CreateAsync(user, dto.Password);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            // Invalida cache de listagem
            await _cache.RemoveAsync("users:all");

            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                new UserDto(user.Id.ToString(), user.Email, user.FullName));
        }

        /// <summary>
        /// Lista todos os usuários.
        /// Usa cache para acelerar leituras frequentes.
        /// </summary>
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            const string key = "users:all";
            var cached = await _cache.GetStringAsync(key);
            if (cached != null)
            {
                var list = JsonSerializer.Deserialize<List<UserDto>>(cached)!;
                return Ok(list);
            }

            var users = _userManager.Users
                .Select(u => new UserDto(u.Id.ToString(), u.Email, u.FullName))
                .ToList();

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(users), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

            return Ok(users);
        }

        /// <summary>
        /// Obtém dados de um usuário pelo ID.
        /// Usa cache para acelerar leituras.
        /// </summary>
        [HttpGet("{id}"), AllowAnonymous] //Authorize
        public async Task<IActionResult> GetById(string id)
        {
            var key = $"user:{id}";
            var cached = await _cache.GetStringAsync(key);
            if (cached != null)
            {
                var dto = JsonSerializer.Deserialize<UserDto>(cached)!;
                return Ok(dto);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = new UserDto(user.Id.ToString(), user.Email, user.FullName);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

            return Ok(result);
        }

        /// <summary>
        /// Atualiza e/ou altera senha de um usuário.
        /// Invalida cache nas chaves relacionadas.
        /// </summary>
        [HttpPut, Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
            }
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            var up = await _userManager.UpdateAsync(user);
            if (!up.Succeeded) return BadRequest(up.Errors);

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var rp = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
                if (!rp.Succeeded) return BadRequest(rp.Errors);
            }

            // Invalida cache
            await _cache.RemoveAsync($"user:{dto.Id}");
            await _cache.RemoveAsync("users:all");

            return NoContent();
        }

        /// <summary>
        /// Remove um usuário pelo ID.
        /// Invalida cache após exclusão.
        /// </summary>
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var del = await _userManager.DeleteAsync(user);
            if (!del.Succeeded) return BadRequest(del.Errors);

            // Invalida cache
            await _cache.RemoveAsync($"user:{id}");
            await _cache.RemoveAsync("users:all");

            return NoContent();
        }
    }
}
