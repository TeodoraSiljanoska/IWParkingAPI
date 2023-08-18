using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleFor(x => x.PlateNumber)
               .NotEmpty().WithMessage("PlateNumber is required")
               .Matches("^(?=.*[A-Z])(?=.*[0-9])[A-Z0-9]+$");

            RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => type == "Car" || type == "Adapted Car")
            .WithMessage("Car type must be either 'Car' or 'Adapted Car'");
        }
    }
}

