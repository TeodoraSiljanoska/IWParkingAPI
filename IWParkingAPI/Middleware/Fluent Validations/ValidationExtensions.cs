using FluentValidation;
using IWParkingAPI.Fluent_Validations.Services.Implementation;
using IWParkingAPI.Fluent_Validations.Services.Interfaces;
using IWParkingAPI.Fluent_Validations.Validators;
using IWParkingAPI.Middleware.Fluent_Validations.Validators;
using IWParkingAPI.Models.Requests;
using System.ComponentModel.DataAnnotations;
namespace IWParkingAPI.Fluent_Validations
{
    public static class ValidationExtensions
    {
        /// <summary>
        ///         Validate provided model property annotations.
        /// </summary>
        public static ValidationModel RunValidation<T>(this T model)
        {
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model, null, null), errors, true);

            return new ValidationModel
            {
                IsValid = isValid,
                Errors = errors
            };
        }


        public static IServiceCollection AddValidator(this IServiceCollection services)
        {
            services.AddSingleton<IValidateService, ValidateService>();
            services.AddSingleton <IValidator<ParkingLotReq>, ParkingLotReqValidator >();
            services.AddSingleton<IValidator<UpdateParkingLotRequest>, UpdateParkingLotRequestValidator>();

            services.AddSingleton<IValidator<RoleRequest>, RoleRequestValidator>();

            services.AddSingleton<IValidator<RequestRequest>, RequestRequestValidator>();

            return services;
        }
    }
}
