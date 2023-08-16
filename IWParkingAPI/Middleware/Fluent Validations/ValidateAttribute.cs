using IWParkingAPI.Fluent_Validations.Services.Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWParkingAPI.Fluent_Validations
{
    public class ValidateAttribute : ResultFilterAttribute
    {
        /// <summary>
        ///     On result executing
        ///     If error happened wrap the response and return it
        ///     Catch error converting messages and return proper message
        /// </summary>
        /// <param name="context">Result executing context</param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var dict = new Dictionary<string, List<string>>();
                foreach (var item in context.ModelState)
                {
                    if (!dict.ContainsKey(item.Key))
                    {
                        dict[item.Key] = new List<string>();
                    }
                    foreach (var err in item.Value.Errors)
                    {
                        if (!string.IsNullOrEmpty(item.Key))
                        {
                            dict[item.Key].Add(
                                err.ErrorMessage.StartsWith("Error converting value")
                                || err.ErrorMessage.StartsWith("Could not convert") ?
                                "Invalid Request Parrametars " :
                            err.ErrorMessage);
                        }
                        else
                        {
                            dict[item.Key].Add("InvalidRequest");
                        }
                    }
                }
                context.Result = new BadRequestObjectResult(ValidateService.GetValidationResponse(dict));
            }
        }
    }
}
