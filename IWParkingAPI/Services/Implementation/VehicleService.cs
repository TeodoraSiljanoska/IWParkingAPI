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
        private readonly GetVehiclesResponse _getresponse;

        public VehicleService(IUnitOfWork<ParkingDbContext> unitOfWork, IUnitOfWork<ParkingDbContextCustom> custom)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _custom = custom;
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _userRepository = _custom.GetGenericRepository<ApplicationUser>();
            _response = new VehicleResponse();
            _getresponse = new GetVehiclesResponse();
        }

        //   [AuthorizeCustom(UserRoles.SuperAdmin)]
        public GetVehiclesResponse GetAllVehicles()
        {
            var vehicles = _vehicleRepository.GetAll();
            if (vehicles.Count() == 0)
            {
                _getresponse.StatusCode = HttpStatusCode.NoContent;
                _getresponse.Message = "There aren't any vehicles.";
                _getresponse.Vehicles = Enumerable.Empty<Vehicle>();

            }
            _getresponse.StatusCode = HttpStatusCode.OK;
            _getresponse.Message = "Vehicles returned successfully";
            _getresponse.Vehicles = vehicles;
            return _getresponse;
        }

        public VehicleResponse AddNewVehicle(VehicleRequest request)
        {
            ApplicationUser existinguser = _userRepository.GetById(request.UserId);
            if (existinguser == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User does not exists.";
                return _response;
            }
            if (existinguser != null && existinguser.IsDeactivated == true)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User is deactivated.";
                return _response;
            }

            if (_vehicleRepository.FindByPredicate(u => u.PlateNumber == request.PlateNumber))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "The plate number already exists.";
                return _response;
            }

            if (request.Type != "Car" && request.Type != "Adapted Car")
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Vehicle Type must be Car or Adapted Car.";
                return _response;
            }

            var vehicle = _mapper.Map<Vehicle>(request);

            var vehiclesOfTheUser = _vehicleRepository.GetAll().Where(v => v.UserId == request.UserId);
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
                if (_vehicleRepository.FindByPredicate(u => u.PlateNumber == request.PlateNumber))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Vehicle with that plate number already exists.";
                    return _response;
                }
            }

            /*  vehicle.PlateNumber = string.IsNullOrEmpty(request.PlateNumber) ? vehicle.PlateNumber : request.PlateNumber;
              vehicle.Type = string.IsNullOrEmpty(request.Type) ? vehicle.Type : request.Type; */


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

        public GetVehiclesResponse GetVehiclesByUserId(int userid)
        {
            ApplicationUser user = _userRepository.GetById(userid);
            var existinguser = _vehicleRepository.FindByPredicate(u => u.UserId == userid);

            if (user == null || user.IsDeactivated == true)
            {
                _getresponse.StatusCode = HttpStatusCode.BadRequest;
                _getresponse.Message = "User does not exist.";
                return _getresponse;
            }
            if (existinguser == false)
            {
                _getresponse.StatusCode = HttpStatusCode.NotFound;
                _getresponse.Message = "This user doesn't own any car.";
                return _getresponse;

            }


            var vehicles = _vehicleRepository.GetAll().Where(v => v.UserId == userid);
            if (vehicles.Count() == 0)
            {
                _getresponse.StatusCode = HttpStatusCode.NotFound;
                _getresponse.Message = "There aren't any vehicles.";
                _getresponse.Vehicles = Enumerable.Empty<Vehicle>();
                return _getresponse;
            }
            _getresponse.StatusCode = HttpStatusCode.OK;
            _getresponse.Message = "Vehicles returned successfully";
            _getresponse.Vehicles = vehicles;
            return _getresponse;
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