using System.Collections.Generic;
using System.Linq;
using Accessory.Builder.WebApi.Exceptions.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accessory.Builder.WebApi.Exceptions.Types;

public class QueryNotValidProblemDetails : ProblemDetails
{
    public List<ValidationErrorDto> ValidationErrors { get; }

    public QueryNotValidProblemDetails(QueryNotValidException exception)
    {
        var notFoundProblem = exception
            .ValidationFailures
            .FirstOrDefault(vf => vf.ErrorCode == ValidationErrorCode.NotFound);

        Status = notFoundProblem != null ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        Type = notFoundProblem != null ? ValidationErrorCode.NotFound : ValidationErrorCode.QueryNotValid;
        Title = "Request parameters didn't validate.";
        ValidationErrors = notFoundProblem != null ?
            new List<ValidationErrorDto> { new ValidationErrorDto(notFoundProblem) }
            : exception.ValidationFailures
                .Select(vf => new ValidationErrorDto(vf))
                .ToList();
    }
}