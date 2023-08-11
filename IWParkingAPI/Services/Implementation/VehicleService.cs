using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class VehicleService : IVehicleService
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<ApplicationUser> _userRepository;
        private readonly IUnitOfWork<ParkingDbContextCustom> _custom;
        private readonly VehicleResponse _response;
        private readonly GetVehiclesResponse _getResponse;
        private const string TypeCar = "Car";
        private const string TypeAdaptedCar = "Adapted Car";

        public VehicleService(IUnitOfWork<ParkingDbContext> unitOfWork, IUnitOfWork<ParkingDbContextCustom> custom)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _custom = custom;
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _userRepository = _custom.GetGenericRepository<ApplicationUser>();
            _response = new VehicleResponse();
            _getResponse = new GetVehiclesResponse();
        }

        public GetVehiclesResponse GetAllVehicles()
        {
            var vehicles = _vehicleRepository.GetAll();
            if (vehicles.Count() == 0)
            {
                _getResponse.StatusCode = HttpStatusCode.NoContent;
                _getResponse.Message = "There aren't any vehicles.";
                _getResponse.Vehicles = Enumerable.Empty<Vehicle>();
                return _getResponse;
            }

            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Vehicles returned successfully";
            _getResponse.Vehicles = vehicles;
            return _getResponse;
        }

        public VehicleResponse AddNewVehicle(VehicleRequest request)
        {
            ApplicationUser existingUser = _userRepository.GetById(request.UserId);
            if (existingUser == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User does not exist.";
                return _response;
            }
            if (existingUser != null && existingUser.IsDeactivated == true)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User is deactivated.";
                return _response;
            }

            var checkExistingPlateNumber = _vehicleRepository.GetAsQueryable(v => v.PlateNumber == request.PlateNumber).FirstOrDefault();
            if (checkExistingPlateNumber != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "The plate number already exists.";
                return _response;
            }

            if (request.Type != TypeCar && request.Type != TypeAdaptedCar)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Vehicle Type must be Car or Adapted Car.";
                return _response;
            }

            var vehicle = _mapper.Map<Vehicle>(request);

            var vehiclesOfTheUser = _vehicleRepository.GetAsQueryable(v => v.UserId == request.UserId).ToList();
            if (vehiclesOfTheUser.Count() == 0)
            {
                vehicle.IsPrimary = true;
            }

            vehicle.TimeCreated = DateTime.Now;
            _vehicleRepository.Insert(vehicle);
            _unitOfWork.Save();

            _response.Vehicle = vehicle;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Vehicle created successfully";
            return _response;
        }

        public VehicleResponse DeleteVehicle(int id)
        {
            Vehicle vehicle = _vehicleRepository.GetById(id);
            if (vehicle == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Vehicle not found";
                return _response;
            }

            _vehicleRepository.Delete(vehicle);
            _unitOfWork.Save();

            _response.Vehicle = vehicle;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Vehicle deleted successfully";
            return _response;
        }

        public VehicleResponse UpdateVehicle(int id, UpdateVehicleRequest request)
        {
            Vehicle vehicle = _vehicleRepository.GetById(id);
            if (vehicle == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Vehicle not found";
                return _response;
            }

            if (request.PlateNumber == null || request.Type == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "PlateNumber and Type are required.";
                return _response;
            }

            if (request.Type != "Car" && request.Type != "Adapted Car")
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Vehicle Type must be Car or Adapted Car.";
                return _response;
            }

            if (vehicle.PlateNumber == request.PlateNumber && vehicle.Type == request.Type)
            {
                _response.Vehicle = vehicle;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "No updates were entered. Please enter the updates";
                return _response;
            }

            if (request.PlateNumber != vehicle.PlateNumber)
            {
                var checkExistingPlateNumber = _vehicleRepository.GetAsQueryable(v => v.PlateNumber == request.PlateNumber).FirstOrDefault();
                if (checkExistingPlateNumber != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Vehicle with that plate number already exists.";
                    return _response;
                }
            }

            vehicle.PlateNumber = (vehicle.PlateNumber == request.PlateNumber) ? vehicle.PlateNumber : request.PlateNumber;
            vehicle.Type = (vehicle.Type == request.Type) ? vehicle.Type : request.Type;
            vehicle.TimeModified = DateTime.Now;

            _vehicleRepository.Update(vehicle);
            _unitOfWork.Save();

            _response.Vehicle = vehicle;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Vehicle updated successfully";

            return _response;
        }

        public VehicleResponse GetVehicleById(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "VehicleId is required.";
                return _response;
            }

            Vehicle vehicle = _vehicleRepository.GetById(id);
            if (vehicle == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Vehicle not found";
                return _response;
            }

            _response.Vehicle = vehicle;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Vehicle returned successfully";
            return _response;
        }

        public GetVehiclesResponse GetVehiclesByUserId(int userId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null || user.IsDeactivated == true)
            {
                _getResponse.Message = "User not found";
                _getResponse.StatusCode = HttpStatusCode.NotFound;
                _getResponse.Vehicles = Enumerable.Empty<Vehicle>();
                return _getResponse;
            }

            var vehicles = _vehicleRepository.GetAsQueryable(x => x.UserId == userId).ToList();

            if (!vehicles.Any())
            {
                _getResponse.Message = "User doesn't have any vehicles.";
                _getResponse.StatusCode = HttpStatusCode.NoContent;
                _getResponse.Vehicles = Enumerable.Empty<Vehicle>();
                return _getResponse;
            }

            _getResponse.Message = "Vehicles returned successfully";
            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Vehicles = vehicles;
            return _getResponse;
        }

        public VehicleResponse MakeVehiclePrimary(int userId, int vehicleId)
        {
            Vehicle vehicle = _vehicleRepository.GetById(vehicleId);
            if (vehicle == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Vehicle not found";
                return _response;
            }

            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User not found";
                return _response;
            }

            if (vehicle.IsPrimary == true)
            {
                _response.Message = "Vehicle is already primary";
                _response.StatusCode = HttpStatusCode.NotModified;
                _response.Vehicle = vehicle;
                return _response;
            }

            var vehiclesOfTheUser = _vehicleRepository.GetAll().Where(v => v.UserId == userId);

            foreach (Vehicle veh in vehiclesOfTheUser)
            {
                veh.IsPrimary = false;
                _vehicleRepository.Update(veh);
            }

            vehicle.IsPrimary = true;
            _vehicleRepository.Update(vehicle);
            _unitOfWork.Save();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Vehicle is made to be primary";
            _response.Vehicle = vehicle;
            return _response;
        }
    }
}