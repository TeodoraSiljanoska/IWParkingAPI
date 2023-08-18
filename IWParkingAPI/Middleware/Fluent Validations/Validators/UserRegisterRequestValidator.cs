using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
    {
        //name surname email pass confpass phone role
        public UserRegisterRequestValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Email address is invalid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{6,}$").WithMessage("Password must be at least 6 characters long," +
                " have at least one non alphanumeric character," +
                " have at least one digit ('0'-'9') " +
                "and have at least one uppercase ('A'-'Z')")
                .Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches("^(\\+389\\d{8}|\\d{9,})$").WithMessage("Phone number is invalid");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required");
        }

    }
}
