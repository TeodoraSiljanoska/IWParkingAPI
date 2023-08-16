using FluentValidation;
using FluentValidation.Results;
using IWParkingAPI.Fluent_Validations.Services.Interfaces;
using IWParkingAPI.Middleware.Exceptions;

namespace IWParkingAPI.Fluent_Validations.Services.Implementation
{
    public class ValidateService : IValidateService
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="provider"></param>
        public ValidateService(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        ///     Validate async via fluent validator
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">Model</param>
        /// <returns>Validation result</returns>
        public async Task<ValidationResult> ValidateAsync<T>(T model)
        {
            var validator = _provider.GetService<IValidator<T>>();
            return await validator.ValidateAsync(model);
        }

        /// <summary>
        /// Validation reponse
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static ResponseBase GetValidationResponse(Dictionary<string, List<string>> errors)
        {
            var validateResults = new List<string>();
            foreach (var prop in errors)
            {
                prop.Value.ForEach(v => validateResults.Add(v));
            }

            var response = new ResponseBase
            {
                Errors = validateResults,
                ErrorType = "ValidationError",
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            return response;
        }
    }
}
