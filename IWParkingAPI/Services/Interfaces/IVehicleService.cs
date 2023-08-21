using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IVehicleService
    {
        public GetVehiclesResponse GetAllVehicles();
        public VehicleDTOResponse AddNewVehicle(VehicleRequest request);
        public VehicleDTOResponse UpdateVehicle(int id, UpdateVehicleRequest request);
        public VehicleDTOResponse DeleteVehicle(int id);
        public VehicleDTOResponse GetVehicleById(int id);
        public MakeVehiclePrimaryResponse MakeVehiclePrimary(int vehicleId);
        public AllVehiclesByUserResponse GetVehiclesByUserId();
    }
}
