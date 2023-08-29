using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class ZoneRequestValidator: AbstractValidator<ZoneRequest>
    {
        public ZoneRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Zone name is required")
                .Matches("^[A-Z][0-9]{2}$");
        }
    }
}
