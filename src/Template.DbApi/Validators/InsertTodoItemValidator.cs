using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.DbApi.Handlers;

namespace Template.DbApi.Validators;
public class InsertTodoValidator : AbstractValidator<InsertTodo>
{
    public InsertTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.DueDate).NotEmpty().Must(m => m.ToUniversalTime().Date >= DateTime.UtcNow.Date).WithMessage("DueDate cannot be in the past.");
    }
}
