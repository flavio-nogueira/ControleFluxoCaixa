// ControleFluxoCaixa.Mongo/Settings/MongoDbSettings.cs
namespace ControleFluxoCaixa.Mongo.Settings
{
    /// <summary>
    /// Configurações para conexão ao MongoDB e nome da coleção.
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>Ex.: "mongodb://localhost:27017"</summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>Ex.: "ControleFluxoCaixaDb"</summary>
        public string DatabaseName { get; set; } = default!;

        /// <summary>Nome da coleção onde salvaremos os lançamentos para relatório.</summary>
        public string CollectionName { get; set; } = default!;
    }
}
