using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class MakeReservationValidator : AbstractValidator<MakeReservationRequest>
    {

        public MakeReservationValidator()
        {
            RuleFor(x => x.PlateNumber)
                   .NotEmpty().WithMessage("PlateNumber is required")
                   .Matches("^(?=.*[A-Z])(?=.*[0-9])[A-Z0-9]+$").WithMessage("Plate number is invalid");

            RuleFor(s => s.StartTime)
                   .NotEmpty().WithMessage("StartTime is required")
                   .Must(BeAValidTime).WithMessage("Invalid StartTime time format");

            RuleFor(s => s.EndTime)
                   .NotEmpty().WithMessage("EndTime is required")
                   .Must(BeAValidTime).WithMessage("Invalid EndTime time format");

            RuleFor(s => s.StartDate.ToString())
                   .NotEmpty().WithMessage("StartDate is required")
                   .Must(BeAValidDate).WithMessage("Invalid StartDate date format");

            RuleFor(s => s.EndDate.ToString())
                    .NotEmpty().WithMessage("EndDate is required")
                    .Must(BeAValidDate).WithMessage("Invalid EndDate date format");

            RuleFor(s => s.ParkingLotId)
                    .NotEmpty().WithMessage("ParkingLotId is required")
                    .GreaterThan(0).WithMessage("ParkingLotId must be greater than 0");
        }

        private bool BeAValidTime(string value)
        {
            TimeSpan time;
            return TimeSpan.TryParse(value, out time);
        }

        private bool BeAValidDate(string value2)
        {
            DateTime date;
            return DateTime.TryParse(value2, out date);
        }

    }
}
