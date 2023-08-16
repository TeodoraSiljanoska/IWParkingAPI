using FluentValidation.Results;
using IWParkingAPI.Middleware.Exceptions;

namespace IWParkingAPI.Fluent_Validations.Services.Interfaces
{
    public class IValidateService
    {
       Task<ValidationResult> ValidateAsync<T>(T model);
       ResponseBase GetValidationResponse(Dictionary<string, List<string>> errors);

    }
}
