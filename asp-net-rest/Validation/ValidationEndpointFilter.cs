using FluentValidation;

namespace AspNetRest.Validation;

public sealed class ValidationEndpointFilter<TRequest> : IEndpointFilter where TRequest : class
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is null)
        {
            return next(context);
        }

        var argument = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (argument is null)
        {
            return Problem(BuildMissingBodyErrors());
        }

        var result = validator.Validate(argument);

        return result.IsValid ? next(context) : Problem(result.ToDictionary());
    }

    private static ValueTask<object?> Problem(IDictionary<string, string[]> errors) =>
        ValueTask.FromResult<object?>(TypedResults.ValidationProblem(errors));

    private static Dictionary<string, string[]> BuildMissingBodyErrors() =>
        new() { [ValidationMessages.RequestField] = [ValidationMessages.BodyRequired] };
}
