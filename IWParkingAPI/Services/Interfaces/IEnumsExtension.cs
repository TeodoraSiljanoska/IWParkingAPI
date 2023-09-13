using IWParkingAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IEnumsExtension<TEnum>
    {
        public string GetDisplayName(TEnum enumValue);

        public string[] GetDisplayNames(TEnum[] enumValues);
       
    }
}
