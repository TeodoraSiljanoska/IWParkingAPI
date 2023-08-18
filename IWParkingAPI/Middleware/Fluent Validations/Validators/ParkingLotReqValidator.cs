using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Fluent_Validations.Validators
{
    public class ParkingLotReqValidator : AbstractValidator<ParkingLotReq>
    {
        public ParkingLotReqValidator()
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

            RuleFor(x => x.WorkingHourFrom.Hours)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("Working Hours From are required")
                .GreaterThanOrEqualTo(0)
                .LessThan(24)
                .WithMessage("Invalid WorkingHoursFrom: Hours should be greater than or equal to 0 and less than 24");

            RuleFor(x => x.WorkingHourFrom.Minutes)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .LessThan(60)
                .WithMessage("Invalid WorkingHoursFrom: Minutes should be greater than or equal to 0 and less than 60");

            RuleFor(x => x.WorkingHourFrom.Seconds)
                 .Cascade(CascadeMode.StopOnFirstFailure)
                 .GreaterThanOrEqualTo(0)
                 .LessThan(60)
                 .WithMessage("Invalid WorkingHoursFrom: Seconds should be greater than or equal to 0 and less than 60");

            RuleFor(x => x.WorkingHourTo.Hours)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("Working Hours To are required")
                .GreaterThanOrEqualTo(0)
                .LessThan(24)
                .WithMessage("Invalid WorkingHoursTo: Hours should be greater than or equal to 0 and less than 24");

            RuleFor(x => x.WorkingHourTo.Minutes)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .LessThan(60)
                .WithMessage("Invalid WorkingHoursTo: Minutes should be greater than or equal to 0 and less than 60");

            RuleFor(x => x.WorkingHourTo.Seconds)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .LessThan(60)
                .WithMessage("Invalid WorkingHoursTo: Seconds should be greater than or equal to 0 and less than 60");

       /*     RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required")
                .GreaterThan(0).WithMessage("UserId should be greater than 0"); */
        }
    }
}
