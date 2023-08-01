using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        public VehicleResponse AddNewVehicle(VehicleRequest request);
        public VehicleResponse UpdateVehicle(int vehicleId, VehicleRequest request);
        public VehicleResponse DeleteVehicle(int vehicleId);
    }
}
