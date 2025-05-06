using FluentValidation.Results;

namespace Accessory.Builder.WebApi.Exceptions.Dtos;

public static class ValidationErrorTypeMapper
{
    public static ValidationErrorTypes? Fill(ValidationFailure validationFailure)
    {
        if (validationFailure.ErrorCode == ValidationFailureErrorCodes.NotEmptyValidator.ToString())
            return ValidationErrorTypes.Required;
        return null;
    }
}