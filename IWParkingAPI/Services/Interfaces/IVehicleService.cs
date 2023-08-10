using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        public GetVehiclesResponse GetAllVehicles();
        public VehicleResponse AddNewVehicle(VehicleRequest request);
        public VehicleResponse UpdateVehicle(int id, UpdateVehicleRequest request);
        public VehicleResponse DeleteVehicle(int id);
        public VehicleResponse GetVehicleById(int id);
        public VehicleResponse MakeVehiclePrimary(int userId, int vehicleId);
        public GetVehiclesResponse GetVehiclesByUserId(int userid);
    }
}
