using FluentAssertions;
using Template.DbApi.Handlers;
using Template.DbApi.Validators;

namespace Template.DbApi.UnitTests;

public class InsertTodoValidatorTests
{
    readonly InsertTodoValidator validator = new InsertTodoValidator();

    [Fact]
    public void CanPassValidRecord()
    {
        var args = new InsertTodo("Title", "Desc", DateTime.UtcNow.Date);
        var result = validator.Validate(args);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TitleCannotBeEmpty(string value)
    {
        var args = new InsertTodo(value, "Desc", DateTime.UtcNow.Date.AddDays(1));
        var result = validator.Validate(args);
        result.IsValid.Should().BeFalse();

        var error = result.Errors.Single();
        error.PropertyName.Should().Be("Title");
        error.ErrorCode.Should().Be("NotEmptyValidator");
    }

    [Fact]
    public void TitleCannotBeOver125()
    {
        var longString = new string('a', 129);
        var args = new InsertTodo(longString, "Desc", DateTime.UtcNow.Date.AddDays(1));
        var result = validator.Validate(args);
        result.IsValid.Should().BeFalse();

        var error = result.Errors.Single();
        error.PropertyName.Should().Be("Title");
        error.ErrorCode.Should().Be("MaximumLengthValidator");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DescriptionCannotBeEmpty(string value)
    {
        var args = new InsertTodo("Title", value, DateTime.UtcNow.Date.AddDays(1));
        var result = validator.Validate(args);
        result.IsValid.Should().BeFalse();

        var error = result.Errors.Single();
        error.PropertyName.Should().Be("Description");
        error.ErrorCode.Should().Be("NotEmptyValidator");
    }

    [Fact]
    public void DescriptionCannotBeOver512()
    {
        var longString = new string('a', 513);
        var args = new InsertTodo("Title", longString, DateTime.UtcNow.Date.AddDays(1));
        var result = validator.Validate(args);
        result.IsValid.Should().BeFalse();

        var error = result.Errors.Single();
        error.PropertyName.Should().Be("Description");
        error.ErrorCode.Should().Be("MaximumLengthValidator");
    }

    [Fact]
    public void DueDateCannotBeInPast()
    {
        var args = new InsertTodo("Title", "Desc", DateTime.UtcNow.Date.AddDays(-1));
        var result = validator.Validate(args);
        result.IsValid.Should().BeFalse();

        var error = result.Errors.Single();
        error.PropertyName.Should().Be("DueDate");
        error.ErrorMessage.Should().Be("DueDate cannot be in the past.");
    }
}