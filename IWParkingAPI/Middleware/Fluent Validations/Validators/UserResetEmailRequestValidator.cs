using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UserResetEmailRequestValidator : AbstractValidator<UserChangeEmailRequest>
    {
        public UserResetEmailRequestValidator() 
        {
            RuleFor(x => x.OldEmail)
                .NotEmpty().WithMessage("Old Email address is required");

            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("New Email address is required")
                .NotEqual(x => x.OldEmail).WithMessage("Old Email address and new Email address should not be equal")
                .EmailAddress().WithMessage("New Email address is invalid");
        }
    }
}
