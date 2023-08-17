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
using IWParkingAPI.CustomExceptions;
using NLog;
using IWParkingAPI.Utilities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IJWTDecode _jWTDecode;



        public VehicleService(IConfiguration configuration, IUnitOfWork<ParkingDbContext> unitOfWork, IUnitOfWork<ParkingDbContextCustom> custom, IHttpContextAccessor httpContextAccessor, IConfiguration config,
            IJWTDecode jWTDecode)
        {
            _config = config;
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _custom = custom;
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _userRepository = _custom.GetGenericRepository<ApplicationUser>();
            _response = new VehicleResponse();
            _getResponse = new GetVehiclesResponse();
            _httpContextAccessor = httpContextAccessor;
            _jWTDecode = jWTDecode;
        }

        public GetVehiclesResponse GetAllVehicles()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAll();

                if (!vehicles.Any())
                {
                    _getResponse.StatusCode = HttpStatusCode.OK;
                    _getResponse.Message = "There aren't any vehicles";
                    _getResponse.Vehicles = Enumerable.Empty<Vehicle>();
                    return _getResponse;
                }

                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Vehicles returned successfully";
                _getResponse.Vehicles = vehicles;
                return _getResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Vehicles {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Vehicles");
            }
        }

        public VehicleResponse AddNewVehicle(VehicleRequest request)
        {
            try
            {
                if (request.UserId <= 0 || request.PlateNumber == null || request.PlateNumber.Length == 0 || request.Type == null || request.Type.Length == 0)
                {
                    throw new BadRequestException("User Id, Plate Number and Type are required");
                }

                var existingUser = _userRepository.GetById(request.UserId);

                if (existingUser == null || existingUser.IsDeactivated == true)
                {
                    throw new NotFoundException("User doesn't exist");
                }

                var checkExistingPlateNumber = _vehicleRepository.GetAsQueryable(v => v.PlateNumber == request.PlateNumber).FirstOrDefault();
                if (checkExistingPlateNumber != null)
                {
                    throw new BadRequestException("Vehicle with that plate number already exists");
                }

                if (request.Type != TypeCar && request.Type != TypeAdaptedCar)
                {
                    throw new BadRequestException("Vehicle type must be Car or Adapted Car");
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
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for AddNewVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for AddNewVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex) 
            {
                _logger.Error($"Internal Error for AddNewVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while creating the Vehicle {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while creating the Vehicle");
            }
        }

        public VehicleResponse DeleteVehicle(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
                }

                var vehicle = _vehicleRepository.GetById(id);
                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                if (vehicle.IsPrimary == true)
                {
                    var vehiclesOfUser = _vehicleRepository.GetAsQueryable(v => v.UserId == vehicle.UserId).ToList();
                    if (vehiclesOfUser != null && vehiclesOfUser.Count() > 1)
                    {
                        vehiclesOfUser.Remove(vehicle);
                        vehiclesOfUser.First().IsPrimary = true;
                    }
                }

                _vehicleRepository.Delete(vehicle);
                _unitOfWork.Save();

                var deletedVehicle = _vehicleRepository.GetAsQueryable(v => v.Id == id).FirstOrDefault();
                if (deletedVehicle != null)
                {
                    throw new InternalErrorException("An error while deleting the Vehicle occurred");
                }

                _response.Vehicle = vehicle;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Vehicle deleted successfully";
                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for DeleteVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for DeleteVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for DeleteVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while deleting the Vehicle {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while deleting the Vehicle");
            }
        }

        public VehicleResponse UpdateVehicle(int id, UpdateVehicleRequest request)
        {
            try
            {
                if (id <= 0 || request.PlateNumber == null || request.PlateNumber.Length == 0 || request.Type == null || request.Type.Length == 0)
                {
                    throw new BadRequestException("Vehicle Id, Plate Number and Type are required");
                }

                var vehicle = _vehicleRepository.GetById(id);
                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                if (request.PlateNumber == null || request.Type == null)
                {
                    throw new BadRequestException("Plate Number and Type are required");
                }

                if (request.Type != TypeCar && request.Type != TypeAdaptedCar)
                {
                    throw new BadRequestException("Vehicle type must be Car or Adapted Car");
                }

                if (vehicle.PlateNumber == request.PlateNumber && vehicle.Type == request.Type)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                if (request.PlateNumber != vehicle.PlateNumber)
                {
                    var checkExistingPlateNumber = _vehicleRepository.GetAsQueryable(v => v.PlateNumber == request.PlateNumber).FirstOrDefault();
                    if (checkExistingPlateNumber != null)
                    {
                        throw new BadRequestException("Vehicle with that plate number already exists");
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
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for UpdateVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for UpdateVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for UpdateVehicle {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while updating the Vehicle {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while updating the Vehicle");
            }
        }

        public VehicleResponse GetVehicleById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
                }

                Vehicle vehicle = _vehicleRepository.GetById(id);

                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                _response.Vehicle = vehicle;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Vehicle returned successfully";
                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetVehicleById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetVehicleById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting the Vehicle by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Vehicle by Id");
            }
        }

        public GetVehiclesResponse GetVehiclesByUserId()
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                 if (userId <= 0)
                  {
                      throw new BadRequestException("User Id is required");
                  }
                
                var user = _userRepository.GetById(userId);
                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                var vehicles = _vehicleRepository.GetAsQueryable(x => x.UserId == userId).ToList();

                if (!vehicles.Any())
                {
                    _getResponse.StatusCode = HttpStatusCode.OK;
                    _getResponse.Message = "User doesn't have any vehicles";
                    _getResponse.Vehicles = Enumerable.Empty<Vehicle>();
                    return _getResponse;
                }

                _getResponse.Message = "Vehicles returned successfully";
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Vehicles = vehicles;
                return _getResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetVehicleByUserId {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetVehicleByUserId {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting the Vehicles by User Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Vehicles by User Id");
            }

        }

        public VehicleResponse MakeVehiclePrimary(int userId, int vehicleId)
        {
            try
            {
                if (userId <= 0 || vehicleId <= 0)
                {
                    throw new BadRequestException("User Id and Vehicle Id are required");
                }

                Vehicle vehicle = _vehicleRepository.GetById(vehicleId);
                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                var user = _userRepository.GetById(userId);

                if (user == null)
                {
                    throw new NotFoundException("User not found");
                }

                var vehiclesOfTheUser = _vehicleRepository.GetAll().Where(v => v.UserId == userId);

                if (!vehiclesOfTheUser.Contains(vehicle))
                {
                    throw new BadRequestException("Vehicle doesn't belong to user");
                }

                if (vehicle.IsPrimary == true)
                {
                    throw new BadRequestException("Vehicle is already primary");
                }

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
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for MakeVehiclePrimary {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for MakeVehiclePrimary {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while making the Vehicle primary {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while making Vehicle primary");
            }
        }
       
    }
}