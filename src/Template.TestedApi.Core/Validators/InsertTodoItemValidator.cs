using FluentValidation;
using Template.TestedApi.Core.Handlers;

namespace Template.TestedApi.Core.Validators;
public class InsertTodoValidator : AbstractValidator<InsertTodo>
{
    public InsertTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.DueDate).NotEmpty().Must(m => m.ToUniversalTime().Date >= DateTime.UtcNow.Date).WithMessage("DueDate cannot be in the past.");
    }
}
