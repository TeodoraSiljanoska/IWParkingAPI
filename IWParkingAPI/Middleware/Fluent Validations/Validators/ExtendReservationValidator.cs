using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class ExtendReservationValidator : AbstractValidator<ExtendReservationRequest>
    {
        public ExtendReservationValidator()
        {
            RuleFor(s => s.EndTime)
                   .NotEmpty().WithMessage("EndTime is required")
                   .Must(BeAValidTime).WithMessage("Invalid EndTime time format");

            RuleFor(s => s.EndDate.ToString())
                   .NotEmpty().WithMessage("EndDate is required")
                   .Must(BeAValidDate).WithMessage("Invalid EndDate date format");
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
