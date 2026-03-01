using App.Api.Todo;
using App.Api.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Api.Tests.Validators;

public class CreateTodoRequestValidatorTests
{
    private readonly CreateTodoRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        var request = new CreateTodoRequest("Buy groceries", false, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        var request = new CreateTodoRequest("", false, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WhitespaceTitle_ShouldFail()
    {
        var request = new CreateTodoRequest("   ", false, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_TitleExceeds200Characters_ShouldFail()
    {
        var longTitle = new string('A', 201);
        var request = new CreateTodoRequest(longTitle, false, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Validate_TitleExactly200Characters_ShouldPass()
    {
        var maxTitle = new string('A', 200);
        var request = new CreateTodoRequest(maxTitle, false, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullDueBy_ShouldPass()
    {
        var request = new CreateTodoRequest("Valid title", false, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_FutureDueDate_ShouldPass()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var request = new CreateTodoRequest("Valid title", false, futureDate);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_PastDueDate_ShouldFail()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var request = new CreateTodoRequest("Valid title", false, pastDate);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DueBy)
            .WithErrorMessage("Due date must be today or in the future");
    }
}

public class UpdateTodoRequestValidatorTests
{
    private readonly UpdateTodoRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        var request = new UpdateTodoRequest("Updated title", true, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        var request = new UpdateTodoRequest("", true, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WhitespaceTitle_ShouldFail()
    {
        var request = new UpdateTodoRequest("   ", false, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_TitleExceeds200Characters_ShouldFail()
    {
        var longTitle = new string('Z', 201);
        var request = new UpdateTodoRequest(longTitle, false, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Validate_TitleExactly200Characters_ShouldPass()
    {
        var maxTitle = new string('Z', 200);
        var request = new UpdateTodoRequest(maxTitle, false, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullDueBy_ShouldPass()
    {
        var request = new UpdateTodoRequest("Valid title", true, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_FutureDueDate_ShouldPass()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var request = new UpdateTodoRequest("Valid title", true, futureDate);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_PastDueDate_ShouldFail()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var request = new UpdateTodoRequest("Valid title", true, pastDate);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DueBy)
            .WithErrorMessage("Due date must be today or in the future");
    }
}
