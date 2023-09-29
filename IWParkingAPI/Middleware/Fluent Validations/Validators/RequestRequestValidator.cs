using FluentValidation;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Enums;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class RequestRequestValidator : AbstractValidator<RequestRequest>
    {
        public RequestRequestValidator() 
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(x => x.Equals(Enums.RequestStatus.Approved.ToString()) || x.Equals(Enums.RequestStatus.Declined.ToString()))
                    .WithMessage("Status name must be either 'Approved' or 'Declined'");
        }
    }
}
