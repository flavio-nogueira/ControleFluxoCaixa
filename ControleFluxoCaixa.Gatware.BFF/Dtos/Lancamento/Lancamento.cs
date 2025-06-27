using ControleFluxoCaixa.BFF.Dtos.Lancamento;

namespace ControleFluxoCaixa.Gatware.BFF.Dtos.Lancamento
{
    public class Lancamento
    {
        public List<LancamentoDto> Itens { get; set; } = new();
    }
}
