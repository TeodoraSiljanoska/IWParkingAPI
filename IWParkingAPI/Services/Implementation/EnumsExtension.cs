using IWParkingAPI.Models.Enums;
using IWParkingAPI.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.Services.Implementation
{
    public class EnumsExtension<TEnum> : IEnumsExtension<TEnum>
    {
        public string GetDisplayName(TEnum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                                            .GetField(enumValue.ToString())
                                            .GetCustomAttributes(typeof(DisplayAttribute), false)
                                            .FirstOrDefault() as DisplayAttribute;

            return displayAttribute?.Name ?? enumValue.ToString();
        }

        public string[] GetDisplayNames(TEnum[] enumValues)
        {
            return enumValues.Select(enumValue =>
            {
                var displayAttribute = enumValue.GetType()
                                                .GetField(enumValue.ToString())
                                                .GetCustomAttributes(typeof(DisplayAttribute), false)
                                                .FirstOrDefault() as DisplayAttribute;

                return displayAttribute?.Name ?? enumValue.ToString();
            }).ToArray();
        }
    }
}
