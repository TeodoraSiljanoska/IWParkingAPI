using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class RoleRequestValidator : AbstractValidator<RoleRequest>
    {
        public RoleRequestValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required");
        }
    }
}
