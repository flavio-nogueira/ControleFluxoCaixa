using System.ComponentModel;

namespace ControleFluxoCaixa.Domain.Enums
{
    /// <summary>
    /// Tipos de lançamento financeiro.
    /// 0 = Débito, 1 = Crédito.
    /// </summary>
    public enum TipoLancamento : int
    {
        /// <summary>
        /// Lançamento de débito, representado pelo valor 0.
        /// Pode representar saída de caixa ou despesa.
        /// </summary>
        [Description("Débito")]
        Debito = 0,

        /// <summary>
        /// Lançamento de crédito, representado pelo valor 1.
        /// Pode representar entrada de caixa ou receita.
        /// </summary>
        [Description("Crédito")]
        Credito = 1
    }
}
