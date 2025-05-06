using System;
using FluentValidation.Results;

namespace Accessory.Builder.WebApi.Exceptions.Types;

public class QueryNotValidException : Exception
{
    public ValidationFailure[] ValidationFailures { get; }

    public QueryNotValidException(params ValidationFailure[] validationFailures)
    {
        ValidationFailures = validationFailures;
    }
}