using FluentValidation;
using IWParkingAPI.Models.Requests;
using static IWParkingAPI.Models.Enums.Enums;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleFor(x => x.PlateNumber)
               .NotEmpty().WithMessage("PlateNumber is required")
               .Matches("^(?=.*[A-Z])(?=.*[0-9])[A-Z0-9]+$").WithMessage("Plate number is invalid");

            RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => Enum.IsDefined(typeof(VehicleTypes), type)).WithMessage("Type is invalid");
        }
    }
}

