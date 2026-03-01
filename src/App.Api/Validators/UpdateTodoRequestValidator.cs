using App.Api.Todo;
using FluentValidation;

namespace App.Api.Validators;

/// <summary>
/// Validator for UpdateTodoRequest.
/// </summary>
public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
{
    public UpdateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DueBy)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DueBy.HasValue)
            .WithMessage("Due date must be today or in the future");
    }
}
