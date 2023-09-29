using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UserResetPasswordRequestValidator : AbstractValidator<UserResetPasswordRequest>
    {
        public UserResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required");

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{6,}$").WithMessage("New password must be at least 6 characters long," +
                " have at least one non alphanumeric character," +
                " have at least one digit ('0'-'9') " +
                "and have at least one uppercase ('A'-'Z')")
                .NotEqual(x => x.OldPassword).WithMessage("Old password and new password should not be equal");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm new password is required")
                .Equal(x => x.NewPassword).WithMessage("The new passwords do not match");
        }
    }
}
