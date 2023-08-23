using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UpdateParkingLotRequestValidator : AbstractValidator<UpdateParkingLotRequest>
    {
        public UpdateParkingLotRequestValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty()
               .WithMessage("Name is required");

            RuleFor(x => x.City)
               .NotEmpty()
               .WithMessage("City is required");

            RuleFor(x => x.Zone)
               .NotEmpty()
               .WithMessage("Zone is required");

            RuleFor(x => x.Address)
               .NotEmpty()
               .WithMessage("Address is required");

            RuleFor(x => x.Price)
               .NotEmpty().WithMessage("Price is required")
               .GreaterThan(0).WithMessage("Price should be greater than 0");

            RuleFor(x => x.CapacityCar)
                .NotEmpty().WithMessage("Car Capacity is required")
                .GreaterThan(0).WithMessage("Capacity should be greater than 0");

            RuleFor(x => x.CapacityAdaptedCar)
                .NotEmpty().WithMessage("Adapted Car Capacity is required")
                .GreaterThan(0).WithMessage("Capacity should be greater than 0");

            RuleFor(x => x.WorkingHourFrom)
                .NotEmpty().WithMessage("Working Hours From are required");

            RuleFor(x => x.WorkingHourTo)
                .NotEmpty().WithMessage("Working Hours To are required");

        }
    }
}