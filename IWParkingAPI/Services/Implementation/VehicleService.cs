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
            catch (Exception)
            {
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
            catch (NotFoundException)
            {
                throw;
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (InternalErrorException)
            {
                throw;
            }
            catch (Exception)
            {
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
                    if (vehiclesOfUser != null && vehiclesOfUser.Count() > 0)
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InternalErrorException)
            {
                throw;
            }
            catch (Exception)
            {
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InternalErrorException)
            {
                throw;
            }
            catch (Exception)
            {
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InternalErrorException("Unexpected error while getting the Vehicle by Id");
            }
        }

        public GetVehiclesResponse GetVehiclesByUserId(int userId)
        {
            try
            {
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InternalErrorException("Unexpected error while making Vehicle primary");
            }
        }
    }
}