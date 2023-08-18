using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        public GetVehiclesResponse GetAllVehicles();
        public VehicleResponseDTO AddNewVehicle(VehicleRequest request);
        public VehicleResponseDTO UpdateVehicle(int id, UpdateVehicleRequest request);
        public VehicleResponseDTO DeleteVehicle(int id);
        public VehicleResponseDTO GetVehicleById(int id);
        public MakeVehiclePrimaryResponse MakeVehiclePrimary(int userId, int vehicleId);
        public GetAllVehiclesByUserIdResponse GetVehiclesByUserId();
    }
}
