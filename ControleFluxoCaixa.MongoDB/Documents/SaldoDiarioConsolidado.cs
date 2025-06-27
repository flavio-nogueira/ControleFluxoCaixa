// ControleFluxoCaixa.Mongo/Documents/SaldoDiarioConsolidado.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ControleFluxoCaixa.Mongo.Documents
{
    /// <summary>
    /// Documento que representa o saldo diário consolidado no MongoDB.
    /// Cada documento é indexado pela data (string "yyyy-MM-dd") e armazena:
    ///   - SaldoAnterior: saldo acumulado até o dia anterior.
    ///   - TotalEntradas: soma dos valores de lançamentos de tipo “entrada” naquele dia.
    ///   - TotalSaidas:   soma dos valores de lançamentos de tipo “saída” naquele dia.
    ///   - Saldo:         SaldoAnterior + TotalEntradas – TotalSaidas.
    /// </summary>
    public class SaldoDiarioConsolidado
    {
        /// <summary>
        /// Usamos a própria string "yyyy-MM-dd" como _id do documento.
        /// </summary>
        [BsonId]
        //[BsonRepresentation(BsonType.DateTime)]
        public string Date { get; set; } = default!;

        /// <summary>
        /// Saldo apurado no dia imediatamente anterior. Se não existir dia anterior, 0m.
        /// </summary>
        [BsonElement("SaldoAnterior")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SaldoAnterior { get; set; }

        /// <summary>
        /// Soma de todos os lançamentos de tipo "entrada" para esta data.
        /// </summary>
        [BsonElement("TotalEntradas")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalEntradas { get; set; }

        /// <summary>
        /// Soma de todos os lançamentos de tipo "saída" para esta data.
        /// </summary>
        [BsonElement("TotalSaidas")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalSaidas { get; set; }

        /// <summary>
        /// Saldo final do dia: SaldoAnterior + TotalEntradas – TotalSaidas.
        /// </summary>
        [BsonElement("Saldo")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Saldo { get; set; }
    }
}
