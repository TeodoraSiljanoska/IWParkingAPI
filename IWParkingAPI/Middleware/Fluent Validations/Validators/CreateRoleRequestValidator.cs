using FluentValidation;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class CreateRoleRequestValidator : AbstractValidator<RoleRequest>
    {
        public CreateRoleRequestValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");
        }
    }
}
