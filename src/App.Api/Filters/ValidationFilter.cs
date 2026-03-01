using FluentValidation;

namespace App.Api.Filters;

/// <summary>
/// Endpoint filter that validates request bodies using FluentValidation.
/// Returns 400 ProblemDetails with validation errors if validation fails.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context).ConfigureAwait(false);
        }

        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
        {
            return Results.Problem(
                detail: "Request body is required",
                statusCode: 400,
                title: "Validation Error"
            );
        }

        var result = await validator.ValidateAsync(argument).ConfigureAwait(false);
        if (!result.IsValid)
        {
            var errors = result.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }
            );

            return Results.ValidationProblem(errors, title: "Validation Error");
        }

        return await next(context).ConfigureAwait(false);
    }
}
