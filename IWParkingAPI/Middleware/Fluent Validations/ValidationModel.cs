using IWParkingAPI.Models.Responses;
using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.Fluent_Validations
{
    public class ValidationModel : ResponseBase
    {
        public ValidationModel()
        {
            Errors = new List<ValidationResult>();
        }

        public bool IsValid { get; set; }

        public IList<ValidationResult> Errors { get; set; }
    }
}
