using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class RequestRequestValidator : AbstractValidator<RequestRequest>
    {
        public RequestRequestValidator() 
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required");
        }
    }
}
