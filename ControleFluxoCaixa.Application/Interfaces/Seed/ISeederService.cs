namespace ControleFluxoCaixa.Application.Interfaces.Seed
{
    /// <summary>
    /// Contrato para serviços responsáveis por registrar e verificar a execução de seeds (dados iniciais).
    /// Essa interface permite controlar quais scripts de seed já foram executados, evitando duplicações.
    /// </summary>
    public interface ISeederService
    {
        /// <summary>
        /// Verifica se um determinado seed já foi executado com sucesso anteriormente.
        /// </summary>
        /// <param name="seedName">Nome único do seed (ex: "SeedUsuarioAdmin").</param>
        /// <returns>Retorna true se o seed já foi executado com sucesso; caso contrário, false.</returns>
        Task<bool> HasRunAsync(string seedName);

        /// <summary>
        /// Registra a execução de um seed, armazenando nome, status e responsável.
        /// </summary>
        /// <param name="seedName">Nome único do seed (ex: "SeedUsuarioAdmin").</param>
        /// <param name="succeeded">Define se a execução foi bem-sucedida (true) ou falhou (false).</param>
        /// <param name="executedBy">Identificação de quem executou o seed (pode ser um e-mail ou "sistema").</param>
        /// <returns>Task que representa a operação assíncrona de persistência.</returns>
        Task MarkAsRunAsync(string seedName, bool succeeded, string executedBy);
    }
}
