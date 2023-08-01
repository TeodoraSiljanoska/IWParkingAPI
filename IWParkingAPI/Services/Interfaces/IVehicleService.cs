using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        public VehicleResponse AddNewVehicle(VehicleRequest request);
        public VehicleResponse UpdateVehicle(int id, VehicleRequest request);
        public VehicleResponse DeleteVehicle(int id);
        public VehicleResponse GetVehicleById(GetVehicleByIdRequest request);
    }
}
