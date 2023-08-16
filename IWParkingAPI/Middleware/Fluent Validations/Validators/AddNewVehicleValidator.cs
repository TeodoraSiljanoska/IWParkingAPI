using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class AddNewVehicleValidator : AbstractValidator<VehicleRequest> 
    {
        public AddNewVehicleValidator()
        {
            RuleFor(x => x.UserId)
               .NotEmpty().WithMessage("UserId is required")
               .GreaterThan(0).WithMessage("UserId should be greater than 0");

            RuleFor(x => x.PlateNumber)
               .NotEmpty()
               .WithMessage("PlateNumber is required");

            //      RuleFor(x => x.Type)
            //         .NotEmpty()
            //         .WithMessage("Type is required")
            //         .Must("")
        }
    }
}
