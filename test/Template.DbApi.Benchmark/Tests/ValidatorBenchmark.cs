using BenchmarkDotNet.Attributes;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.DbApi.Handlers;
using Template.DbApi.Validators;

namespace Template.DbApi.Benchmark.Tests;

public class ValidatorBenchmark
{
    private readonly InsertTodoValidator subject = new InsertTodoValidator();
    private readonly InsertTodo validPayload;
    private readonly InsertTodo invalidPayload;

    public ValidatorBenchmark()
    {
        validPayload = new InsertTodo("Title", "Description", DateTime.Today);
        invalidPayload = new InsertTodo(string.Empty, new string('a', 550), DateTime.Today.AddDays(-1));
    }

    [Benchmark]
    public ValidationResult ValidBenchmark() => subject.Validate(validPayload);

    [Benchmark]
    public ValidationResult InvalidBenchmark() => subject.Validate(invalidPayload);
}