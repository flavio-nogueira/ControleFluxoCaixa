using ControleFluxoCaixa.Application.Commands.Lancamento;
using ControleFluxoCaixa.Application.DTOs;
using FluentValidation;

namespace ControleFluxoCaixa.Application.Validators.Lancamento
{
    /// <summary>
    /// Validador para o comando CreateLancamentoCommand.
    /// Garante que a lista de lançamentos não seja nula ou vazia
    /// e aplica validação em cada item da lista usando LancamentoDtoValidator.
    /// </summary>
    public class CreateLancamentoCommandValidator : AbstractValidator<CreateLancamentoCommand>
    {
        public CreateLancamentoCommandValidator()
        {
            // A lista LancamentoDtos não pode ser nula
            RuleFor(x => x.Itens)
                .NotNull().WithMessage("A lista de lançamentos não pode ser nula.")

                // A lista também não pode estar vazia
                .NotEmpty().WithMessage("A lista de lançamentos não pode estar vazia.");

            // Para cada item da lista, aplica o validador personalizado LancamentoDtoValidator
            RuleForEach(x => x.Itens)
                .SetValidator(new LancamentoDtoValidator());
        }
    }

    /// <summary>
    /// Validador para o DTO de lançamento (LancamentoDto).
    /// Garante que todos os campos obrigatórios estejam corretos.
    /// </summary>
    public class LancamentoDtoValidator : AbstractValidator<Itens>
    {
        public LancamentoDtoValidator()
        {
            // Regra: Data deve ser informada e não pode ser o valor padrão
            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("A data é obrigatória.")
                .Must(d => d != default).WithMessage("A data deve ser válida.");

            // Regra: Valor deve ser maior que zero
            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");

            // Regra: Descrição não pode ser nula ou vazia
            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.");

            // Regra: Tipo deve estar dentro do enum TipoLancamento
            RuleFor(x => x.Tipo)
                .IsInEnum().WithMessage("Tipo de lançamento inválido.");
        }
    }
}
