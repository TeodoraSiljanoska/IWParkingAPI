using System.Net;
using FluentValidation.Results;

namespace IWParkingAPI.Middleware.Exceptions
{
    public class ResponseBase
    {
        public ResponseBase()
        {
            Errors = new List<string>();
        }

        /// <summary>
        ///     Errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        ///     ErrorType
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        ///     StatusCode
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Indicates whether or not the call was successful, based on the HTTP response code.
        /// </summary>
        public bool IsSuccessful => ((int)StatusCode >= 200) && ((int)StatusCode <= 299) && Errors.Count == 0;

        /// <summary>
        /// Indicates whether or not the response has errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Correctly configures the response when validation errors are present.
        /// </summary>
        /// <param name="result"><see cref="ValidationResult"/></param>
        public void ConfigureResponseForValidationErrors(ValidationResult result)
        {
            if (!result.IsValid)
            {
                StatusCode = HttpStatusCode.BadRequest;
                Errors = result.Errors
                    .Select(x => x.ErrorMessage)
                    .ToList();
            }
        }

        /// <summary>
        /// Correctly configures the response when validation error is present.
        /// </summary>
        /// <param name="error"><see cref="string"/></param>
        public void ConfigureResponseForValidationErrors(string error)
        {
            StatusCode = HttpStatusCode.BadRequest;
            Errors = new List<string> { error };
        }

        /// <summary>
        /// Correctly configures the response when Exception error is present.
        /// </summary>
        /// <param name="error"><see cref="string"/></param>
        public void ConfigureResponseForExceptionErrors(string error)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Errors = new List<string> { error };
        }


    }
}
