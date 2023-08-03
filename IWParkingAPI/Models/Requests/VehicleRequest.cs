using AutoMapper.Configuration.Annotations;

namespace IWParkingAPI.Models.Requests
{
    public class VehicleRequest
    {
        public int UserId { get; set; }


        public string PlateNumber { get; set; } = null!;

        public string Type { get; set; } = null!;
    }
}
