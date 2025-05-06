using Accessory.Builder.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accessory.Builder.WebApi.Exceptions.Types;

public class BrokenBusinessRuleProblemDetails : ProblemDetails
{
    public BrokenBusinessRuleProblemDetails(BrokenBusinessRuleException exception)
    {
        Title = exception.Code;
        Status = StatusCodes.Status400BadRequest;
        Detail = exception.Details;
        Type = "broken-business-rule/" + exception.Code;
    }
}