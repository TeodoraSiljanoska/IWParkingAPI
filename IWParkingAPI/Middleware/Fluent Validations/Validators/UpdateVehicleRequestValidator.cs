using FluentValidation;
using IWParkingAPI.Models.Requests;
using System.ComponentModel.DataAnnotations;
using static IWParkingAPI.Models.Enums.Enums;

namespace IWParkingAPI.Middleware.Fluent_Validations.Validators
{
    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleFor(x => x.PlateNumber)
               .NotEmpty().WithMessage("PlateNumber is required")
               .Matches("^(?=.*[A-Z])(?=.*[0-9])[A-Z0-9]+$").WithMessage("Plate number is invalid");

            RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(typeDisplayName =>
            {
                var enumType = typeof(VehicleTypes);
                foreach (var field in enumType.GetFields())
                {
                    if (field.IsStatic)
                    {
                        var displayAttribute = field.GetCustomAttributes(typeof(DisplayAttribute), false)
                            .FirstOrDefault() as DisplayAttribute;

                        if (displayAttribute != null && displayAttribute.Name == typeDisplayName)
                        {
                            return true;
                        }
                    }
                }
                return false;
            })
            .WithMessage("Type is invalid");
        }
    }
}

