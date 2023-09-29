using FluentValidation.Results;
using IWParkingAPI.Middleware.Exceptions;

namespace IWParkingAPI.Fluent_Validations.Services.Interfaces
{
    public interface IValidateService
    {
       Task<ValidationResult> ValidateAsync<T>(T model);

    }
}
