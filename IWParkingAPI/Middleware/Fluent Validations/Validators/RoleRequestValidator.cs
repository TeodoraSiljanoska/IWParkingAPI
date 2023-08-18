using FluentValidation;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class RoleRequestValidator : AbstractValidator<RoleRequest>
    {
        public RoleRequestValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .Must(x => x.Equals(UserRoles.User) || x.Equals(UserRoles.Owner) || x.Equals(UserRoles.SuperAdmin))
                    .WithMessage("Role name must be either 'User', 'Owner' or 'SuperAdmin'");
        }
    }
}
