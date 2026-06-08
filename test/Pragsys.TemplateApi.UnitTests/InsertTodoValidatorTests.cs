using Shouldly;
using Pragsys.TemplateApi.Core.Handlers;
using Pragsys.TemplateApi.Core.Validators;

namespace Pragsys.TemplateApi.UnitTests;

public class InsertTodoValidatorTests
{
    private readonly InsertTodoValidator _validator = new InsertTodoValidator();

    [Fact]
    public void CanPassValidRecord()
    {
        var args = new InsertTodo("Title", "Desc", DateTime.UtcNow.Date);
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TitleCannotBeEmpty(string? value)
    {
        var args = new InsertTodo(value, "Desc", DateTime.UtcNow.Date.AddDays(1));
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeFalse();

        var error = result.Errors.Single();
        error.PropertyName.ShouldBe("Title");
        error.ErrorCode.ShouldBe("NotEmptyValidator");
    }

    [Fact]
    public void TitleCannotBeOver128()
    {
        var longString = new string('a', 129);
        var args = new InsertTodo(longString, "Desc", DateTime.UtcNow.Date.AddDays(1));
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeFalse();

        var error = result.Errors.Single();
        error.PropertyName.ShouldBe("Title");
        error.ErrorCode.ShouldBe("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DescriptionCannotBeEmpty(string? value)
    {
        var args = new InsertTodo("Title", value, DateTime.UtcNow.Date.AddDays(1));
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeFalse();

        var error = result.Errors.Single();
        error.PropertyName.ShouldBe("Description");
        error.ErrorCode.ShouldBe("NotEmptyValidator");
    }

    [Fact]
    public void DescriptionCannotBeOver512()
    {
        var longString = new string('a', 513);
        var args = new InsertTodo("Title", longString, DateTime.UtcNow.Date.AddDays(1));
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeFalse();

        var error = result.Errors.Single();
        error.PropertyName.ShouldBe("Description");
        error.ErrorCode.ShouldBe("MaximumLengthValidator");
    }

    [Fact]
    public void DueDateCannotBeInPast()
    {
        var args = new InsertTodo("Title", "Desc", DateTime.UtcNow.Date.AddDays(-1));
        var result = _validator.Validate(args);
        result.IsValid.ShouldBeFalse();

        var error = result.Errors.Single();
        error.PropertyName.ShouldBe("DueDate");
        error.ErrorMessage.ShouldBe("DueDate cannot be in the past.");
    }
}
