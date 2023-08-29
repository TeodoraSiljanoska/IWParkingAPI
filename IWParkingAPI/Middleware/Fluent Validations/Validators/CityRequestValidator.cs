using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class CityRequestValidator : AbstractValidator<CityRequest>
    {
        public CityRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("City name is required")
                .Matches("^[A-Z][a-z]*$").WithMessage("First letter must be uppercase");

        }
    }
}
