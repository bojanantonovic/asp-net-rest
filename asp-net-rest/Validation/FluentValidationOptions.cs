using FluentValidation;
using Microsoft.Extensions.Options;

namespace AspNetRest.Validation;

public sealed class FluentValidationOptions<TOptions>(IServiceScopeFactory scopeFactory)
    : IValidateOptions<TOptions> where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        using var scope = scopeFactory.CreateScope();

        var validator = scope.ServiceProvider.GetService<IValidator<TOptions>>();
        if (validator is null)
        {
            return ValidateOptionsResult.Skip;
        }

        var result = validator.Validate(options);

        return result.IsValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(result.Errors.Select(GetErrorMessage));
    }

    private static string GetErrorMessage(FluentValidation.Results.ValidationFailure failure) =>
        failure.ErrorMessage;
}
