namespace ControleFluxoCaixa.Messaging.Contracts
{
    public class SaldoDto
    {
        /// <summary>Data e hora do movimento.</summary>
        public DateTime Data { get; set; }

        /// <summary>Valor do movimento.</summary>
        public decimal Valor { get; set; }

        /// <summary>0 = Crédito (entrada), 1 = Débito (saída).</summary>
        public int Tipo { get; set; }
    }
}
