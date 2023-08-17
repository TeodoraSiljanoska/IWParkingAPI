using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UserResetEmailRequestValidator : AbstractValidator<UserChangeEmailRequest>
    {
        public UserResetEmailRequestValidator() 
        {
            RuleFor(x => x.OldEmail)
                .NotEmpty().WithMessage("Old email is required");

            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("New email is required")
                .NotEqual(x => x.OldEmail).WithMessage("Old email and new email should not be equal")
                .EmailAddress().WithMessage("Email address is invalid");
        }
    }
}
