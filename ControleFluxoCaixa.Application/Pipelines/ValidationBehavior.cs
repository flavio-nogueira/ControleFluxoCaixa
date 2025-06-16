using FluentValidation;
using MediatR;

namespace ControleFluxoCaixa.Application.Pipelines
{
    /// <summary>
    /// Pipeline do MediatR para executar validações FluentValidation automaticamente.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var errors = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(e => e != null)
                    .ToList();

                if (errors.Any())
                    throw new ValidationException(errors);
            }

            return await next();
        }
    }
}
