using FluentValidation.Results;
using FluentValidation;

namespace IWParkingAPI.Fluent_Validations
{
    public static class ValidationExtensions
    {
        public static ValidationModel RunValidation<T> (this T model)
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

            return services;
        }
    }
}
