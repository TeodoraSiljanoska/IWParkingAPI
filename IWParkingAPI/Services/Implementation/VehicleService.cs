using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
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

        public VehicleService(IUnitOfWork<ParkingDbContext> unitOfWork, IUnitOfWork<ParkingDbContextCustom> custom)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _custom = custom;
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _userRepository = _custom.GetGenericRepository<ApplicationUser>();
            _response = new VehicleResponse();
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
            if(existinguser != null && existinguser.IsDeactivated == true)
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


        public VehicleResponse UpdateVehicle(int id, VehicleRequest request)
        {   
           /* if(request.UserId == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "UserId is required.";
                return _response;
            }

            ApplicationUser existinguser = _userRepository.GetById(request.UserId);
            
            if (existinguser == null || existinguser.IsDeactivated == true)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User does not exist.";
                return _response;
            } */

            Vehicle vehicle = _vehicleRepository.GetById(id);
            if (vehicle == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Vehicle not found";
                return _response;
            }
            

                if (_vehicleRepository.FindByPredicate(u => u.PlateNumber == request.PlateNumber))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Vehicle with that plate number already exists.";
                    return _response;
                }

                if(request.Type != null && (request.Type != "Car" && request.Type != "Adapted Car"))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Vehicle Type must be Car or Adapted Car.";
                return _response;
            }


            vehicle.PlateNumber = string.IsNullOrEmpty(request.PlateNumber) ? vehicle.PlateNumber : request.PlateNumber;
            vehicle.Type = string.IsNullOrEmpty(request.Type) ? vehicle.Type : request.Type;
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

    }
}
