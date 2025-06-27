using System;
using System.ComponentModel.DataAnnotations;

namespace ControleFluxoCaixa.BFF.Dtos.Auth
{
    /// <summary>
    /// DTO que representa os tokens de acesso e atualização (refresh),
    /// bem como a data de expiração do token de acesso.
    /// </summary>
    public class RefreshDto
    {
        /// <summary>
        /// Token de acesso JWT usado para autenticação em chamadas subsequentes.
        /// </summary>
        [Required(ErrorMessage = "O AccessToken é obrigatório.")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token de atualização (refresh token), usado para obter um novo AccessToken após expiração.
        /// </summary>
        [Required(ErrorMessage = "O RefreshToken é obrigatório.")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora (UTC) em que o AccessToken expira.
        /// </summary>
        [Required(ErrorMessage = "A data de expiração é obrigatória.")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>Construtor vazio necessário para Model Binding.</summary>
        public RefreshDto() { }

        /// <summary>Inicializa o DTO com todos os campos.</summary>
        public RefreshDto(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }
    }
}
