namespace ControleFluxoCaixa.Application.Settings
{
    namespace ControleFluxoCaixa.Application.Settings
    {
        /// <summary>
        /// Representa as configurações do Loki extraídas do appsettings.json.
        /// </summary>
        public class LokiSettings
        {
            /// <summary>URL de envio dos logs para o Loki.</summary>
            public string Uri { get; set; } = string.Empty;

            /// <summary>Caminho local do buffer de logs.</summary>
            public string BufferPath { get; set; } = string.Empty;

            /// <summary>Frequência de envio em segundos.</summary>
            public int PeriodSeconds { get; set; } = 10;
        }
    }

}
