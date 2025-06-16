using ControleFluxoCaixa.Mongo.Documents;
using ControleFluxoCaixa.Mongo.Settings;
using ControleFluxoCaixa.MongoDB.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;

namespace ControleFluxoCaixa.Mongo.Repositories
{
    public class SaldoDiarioConsolidadoRepository : ISaldoDiarioConsolidadoRepository
    {
        private readonly IMongoCollection<SaldoDiarioConsolidado> _collection; // Coleção tipada para gravação final
        private readonly IMongoCollection<BsonDocument> _colecaoRaw; // Coleção bruta para leitura dos dados atuais
        public SaldoDiarioConsolidadoRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbSettings> mongoOptions)
        {
            var settings = mongoOptions.Value; // Pega configurações de conexão
            var database = mongoClient.GetDatabase(settings.DatabaseName); // Obtém o banco

            _collection = database.GetCollection<SaldoDiarioConsolidado>(settings.CollectionName);
            _colecaoRaw = database.GetCollection<BsonDocument>(settings.CollectionName);
     
            var indexKeys = Builders<SaldoDiarioConsolidado>.IndexKeys.Ascending(x => x.Date);
            _collection.Indexes.CreateOne(new CreateIndexModel<SaldoDiarioConsolidado>(indexKeys));
        }
        public async Task UpdateAsync(DateTime data,decimal valor,string tipoFila,bool isEntrada,CancellationToken cancellationToken)
        {
            // Formata a data como chave (_id) do documento no formato yyyy-MM-dd
            var diaString = data.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Cria o filtro para buscar o documento do dia atual
            var filterHoje = Builders<SaldoDiarioConsolidado>.Filter.Eq(x => x.Date, diaString);

            // Lê o documento bruto atual (caso exista) para extrair entradas/saídas anteriores
            var hojeRaw = await _colecaoRaw
                .Find(Builders<BsonDocument>.Filter.Eq("_id", diaString))
                .FirstOrDefaultAsync(cancellationToken);

            decimal antigasEntradas = 0m;
            decimal antigasSaidas = 0m;

            if (hojeRaw != null)
            {
                if (hojeRaw.Contains("TotalEntradas"))
                    antigasEntradas = ConvertToDecimal(hojeRaw["TotalEntradas"]);

                if (hojeRaw.Contains("TotalSaidas"))
                    antigasSaidas = Math.Abs(ConvertToDecimal(hojeRaw["TotalSaidas"]));
            }
            else
            {
                // Recupera o documento anterior mais próximo
                var docAnterior = await _collection
                    .Find(Builders<SaldoDiarioConsolidado>.Filter.Lt(x => x.Date, diaString))
                    .SortByDescending(x => x.Date)
                    .Limit(1)
                    .FirstOrDefaultAsync(cancellationToken);

                if (docAnterior != null)
                {
                    // Usa os campos do documento anterior
                    antigasEntradas = docAnterior.TotalEntradas;
                    antigasSaidas = docAnterior.TotalSaidas;
                }
            }

            // Busca o saldo do último dia anterior
            decimal saldoAnterior = await ObterUltimoSaldoAsync(data.Date, cancellationToken);

            // Calcula magnitude do valor atual (absoluto)
            decimal magnitude = Math.Abs(valor);

            // Inicializa variáveis com valores padrões para evitar erros de compilação
            decimal novasEntradas = antigasEntradas;
            decimal novasSaidas = antigasSaidas;
            decimal totalSaidasNeg = -Math.Abs(novasSaidas);
            decimal novoSaldo = saldoAnterior;

            // Verifica o tipo da fila para aplicar lógica correta
            if (tipoFila.Contains("excluido", StringComparison.OrdinalIgnoreCase))
            {
                if (isEntrada)
                {
                    // Excluindo uma ENTRADA: subtrai dos totais e do saldo
                    novasEntradas = Math.Max(0, antigasEntradas - magnitude);
                    novoSaldo = saldoAnterior - magnitude;
                }
                else
                {
                    // Excluindo uma SAÍDA: subtrai das saídas e soma no saldo
                    novasSaidas = Math.Max(0, antigasSaidas - magnitude);
                    novoSaldo = saldoAnterior + magnitude;
                }
            }
            else if (tipoFila.Contains("inclusao", StringComparison.OrdinalIgnoreCase))
            {
                if (isEntrada)
                {
                    // Incluindo uma ENTRADA: soma nos totais e no saldo
                    novasEntradas += magnitude;
                    novoSaldo = saldoAnterior + magnitude;
                }
                else
                {
                    // Incluindo uma SAÍDA: soma nas saídas e subtrai do saldo
                    novasSaidas += magnitude;
                    novoSaldo = saldoAnterior - magnitude;
                }
            }
            else
            {
                throw new InvalidOperationException($"Tipo de fila '{tipoFila}' não reconhecido. Esperado: 'inclusao' ou 'exclusao'.");
            }

            // Corrige o campo TotalSaidas (sempre negativo)
            totalSaidasNeg = -Math.Abs(novasSaidas);

            // Monta o documento final consolidado
            var docCorrigido = new SaldoDiarioConsolidado
            {
                Date = diaString,
                SaldoAnterior = saldoAnterior,
                TotalEntradas = novasEntradas,
                TotalSaidas = totalSaidasNeg,
                Saldo = novoSaldo
            };

            // Persiste no MongoDB com upsert
            var replaceOptions = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filterHoje, docCorrigido, replaceOptions, cancellationToken);
        }
        private async Task<decimal> ObterUltimoSaldoAsync(DateTime dataAtual, CancellationToken cancellationToken)
        {
            var dataAtualStr = dataAtual.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Verifica se já existe documento com a data atual
            var docHoje = await _collection
                .Find(Builders<SaldoDiarioConsolidado>.Filter.Eq(x => x.Date, dataAtualStr))
                .FirstOrDefaultAsync(cancellationToken);

            if (docHoje != null)
                return docHoje.Saldo;

            // Caso não exista, busca o último saldo anterior
            var docAnterior = await _collection
                .Find(Builders<SaldoDiarioConsolidado>.Filter.Lt(x => x.Date, dataAtualStr))
                .SortByDescending(x => x.Date)
                .Limit(1)
                .FirstOrDefaultAsync(cancellationToken);

            return docAnterior?.Saldo ?? 0m;
        }
        private decimal ConvertToDecimal(BsonValue campo)
        {
            return campo.BsonType switch
            {
                BsonType.Decimal128 => (decimal)campo.AsDecimal128,
                BsonType.Double => (decimal)campo.AsDouble,
                BsonType.Int32 => campo.AsInt32,
                BsonType.Int64 => campo.AsInt64,
                BsonType.String => decimal.TryParse(campo.AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m,
                _ => 0m
            };
        }

        public async Task<List<SaldoDiarioConsolidado>> GetBetweenAsync(
    DateTime dataInicial,
    DateTime dataFinal,
    CancellationToken cancellationToken)
        {
            var startString = dataInicial.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endString = dataFinal.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var filter = Builders<SaldoDiarioConsolidado>.Filter.Gte("Date", startString) &
                         Builders<SaldoDiarioConsolidado>.Filter.Lte("Date", endString);

            var sort = Builders<SaldoDiarioConsolidado>.Sort.Ascending("Date");

            return await _collection
                .Find(filter)
                .Sort(sort)
                .ToListAsync(cancellationToken);
        }




    }
}
