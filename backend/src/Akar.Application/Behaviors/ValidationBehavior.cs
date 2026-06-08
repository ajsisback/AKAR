using Akar.Shared;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Akar.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count != 0)
        {
            var errorCodes = string.Join(", ", failures.Select(f => f.ErrorMessage));
            _logger.LogWarning("Validation failed for {RequestType}: {ErrorCodes}", typeof(TRequest).Name, errorCodes);

            // Try to return a Result<T>.Failure if TResponse is a generic Result type
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse);
                var failureMethod = resultType.GetMethod("Failure")!;
                var firstError = failures.First().ErrorMessage;
                return (TResponse)failureMethod.Invoke(null, [firstError, string.Join("; ", failures.Select(f => f.ErrorMessage))])!;
            }

            // Try to return a non-generic Result.Failure if TResponse is Result
            if (typeof(TResponse) == typeof(Result))
            {
                var firstError = failures.First().ErrorMessage;
                return (TResponse)(object)Result.Failure(firstError, string.Join("; ", failures.Select(f => f.ErrorMessage)));
            }

            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
