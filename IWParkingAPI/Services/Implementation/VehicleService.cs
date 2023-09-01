﻿using AutoMapper;
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
using Microsoft.EntityFrameworkCore;
using IWParkingAPI.Utilities;
using IWParkingAPI.Models.Responses.Dto;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IWParkingAPI.Models.Enums;
using static IWParkingAPI.Models.Enums.Enums;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System;

namespace IWParkingAPI.Services.Implementation
{
    public class VehicleService : IVehicleService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly AllVehiclesWithUserResponse _getResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJWTDecode _jWTDecode;
        private readonly AllVehiclesResponse _vehiclesByUserIdResponse;
        private readonly VehicleResponse _makePrimaryResponse;
        private readonly VehicleTypesResponse _vehicleTypeResponse;



        public VehicleService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _getResponse = new AllVehiclesWithUserResponse();
            _jWTDecode = jWTDecode;
            _vehiclesByUserIdResponse = new AllVehiclesResponse();
            _makePrimaryResponse = new VehicleResponse();
            _vehicleTypeResponse = new VehicleTypesResponse();
        }

        public AllVehiclesWithUserResponse GetAllVehicles()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAsQueryable(null, null, x => x.Include(y => y.User).Include(y => y.User.Roles)).ToList();

                if (!vehicles.Any())
                {
                    _getResponse.StatusCode = HttpStatusCode.OK;
                    _getResponse.Message = "There aren't any vehicles";
                    _getResponse.Vehicles = Enumerable.Empty<VehicleWithUserDTO>();
                    return _getResponse;
                }
                var GetAllVehiclesDTOList = new List<VehicleWithUserDTO>();

                foreach (var p in vehicles)
                {
                    if (p.User.IsDeactivated == false)
                        GetAllVehiclesDTOList.Add(_mapper.Map<VehicleWithUserDTO>(p));
                }

                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Vehicles returned successfully";
                _getResponse.Vehicles = GetAllVehiclesDTOList;
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
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                var existingUser = _userRepository.GetAsQueryable(u => u.Id == userId, null, null).FirstOrDefault();

                if (existingUser == null || existingUser.IsDeactivated == true)
                {
                    throw new NotFoundException("User doesn't exist");
                }

                var checkExistingPlateNumber = _vehicleRepository.GetAsQueryable(v => v.PlateNumber == request.PlateNumber).FirstOrDefault();
                if (checkExistingPlateNumber != null)
                {
                    throw new BadRequestException("Vehicle with that plate number already exists");
                }

                var vehicle = _mapper.Map<Vehicle>(request);

                var vehiclesOfTheUser = _vehicleRepository.GetAsQueryable(v => v.UserId == userId).ToList();
                if (vehiclesOfTheUser.Count() == 0)
                {
                    vehicle.IsPrimary = true;
                }

                vehicle.TimeCreated = DateTime.Now;
                vehicle.UserId = userId;
                _vehicleRepository.Insert(vehicle);
                _unitOfWork.Save();

                var VehicleDTO = _mapper.Map<VehicleDTO>(vehicle);

                _makePrimaryResponse.Vehicle = VehicleDTO;
                _makePrimaryResponse.StatusCode = HttpStatusCode.OK;
                _makePrimaryResponse.Message = "Vehicle created successfully";
                return _makePrimaryResponse;
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
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);

                if (id <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
                }

                var vehicle = _vehicleRepository.GetAsQueryable(x => x.Id == id && x.UserId == userId, null, x => x.Include(y => y.User)).FirstOrDefault();
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

                var deletedVehicle = _vehicleRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User)).FirstOrDefault();
                if (deletedVehicle != null)
                {
                    throw new InternalErrorException("An error while deleting the Vehicle occurred");
                }

                var VehicleDTO = _mapper.Map<VehicleDTO>(vehicle);

                _makePrimaryResponse.Vehicle = VehicleDTO;
                _makePrimaryResponse.StatusCode = HttpStatusCode.OK;
                _makePrimaryResponse.Message = "Vehicle deleted successfully";
                return _makePrimaryResponse;
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
                if (id <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
                }

                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);

                var vehicle = _vehicleRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User)).FirstOrDefault();
                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                if (vehicle.UserId != userId)
                {
                    throw new BadRequestException("You do not own this vehicle");
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

                var VehicleDTO = _mapper.Map<VehicleDTO>(vehicle);

                _makePrimaryResponse.Vehicle = VehicleDTO;
                _makePrimaryResponse.StatusCode = HttpStatusCode.OK;
                _makePrimaryResponse.Message = "Vehicle updated successfully";
                return _makePrimaryResponse;
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
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);

                if (id <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
                }

                var vehicle = _vehicleRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User)).FirstOrDefault();

                if (vehicle == null)
                {
                    throw new NotFoundException("Vehicle not found");
                }

                if (vehicle.UserId != userId)
                {
                    throw new BadRequestException("You do not own this vehicle");
                }

                var VehicleDTO = _mapper.Map<VehicleDTO>(vehicle);

                _makePrimaryResponse.Vehicle = VehicleDTO;
                _makePrimaryResponse.StatusCode = HttpStatusCode.OK;
                _makePrimaryResponse.Message = "Vehicle returned successfully";
                return _makePrimaryResponse;
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

        public AllVehiclesResponse GetVehiclesByUserId()
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                var user = _userRepository.GetById(userId);
                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                var vehicles = _vehicleRepository.GetAsQueryable(x => x.UserId == userId).ToList();

                if (!vehicles.Any())
                {
                    _vehiclesByUserIdResponse.StatusCode = HttpStatusCode.OK;
                    _vehiclesByUserIdResponse.Message = "User doesn't have any vehicles";
                    _vehiclesByUserIdResponse.Vehicles = Enumerable.Empty<VehicleDTO>();
                    return _vehiclesByUserIdResponse;
                }

                var GetAllVehiclesDTOList = new List<VehicleDTO>();
                foreach (var p in vehicles)
                {
                    GetAllVehiclesDTOList.Add(_mapper.Map<VehicleDTO>(p));
                }

                _vehiclesByUserIdResponse.StatusCode = HttpStatusCode.OK;
                _vehiclesByUserIdResponse.Message = "Vehicles returned successfully";
                _vehiclesByUserIdResponse.Vehicles = GetAllVehiclesDTOList;
                return _vehiclesByUserIdResponse;
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

        public VehicleTypesResponse GetVehicleTypes()
        {
            try
            {
                VehicleTypes[] enumValues = Enum.GetValues(typeof(VehicleTypes))
                                                .Cast<VehicleTypes>()
                                                .ToArray();

                string[] displayNames = GetDisplayNames(enumValues);
                List<string> vehicleTypes = new List<string>();

                foreach (string displayName in displayNames)
                {
                    vehicleTypes.Add(displayName);
                }

                _vehicleTypeResponse.StatusCode = HttpStatusCode.OK;
                _vehicleTypeResponse.Message = "Vehicle Types returned successfully";
                _vehicleTypeResponse.VehicleTypes = vehicleTypes;
                return _vehicleTypeResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting the Vehicle types {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Vehicle types");
            }

        }

        public VehicleResponse MakeVehiclePrimary(int vehicleId)
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                if (vehicleId <= 0)
                {
                    throw new BadRequestException("Vehicle Id is required");
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

                var vehicleForResponse = _mapper.Map<VehicleDTO>(vehicle);

                _makePrimaryResponse.StatusCode = HttpStatusCode.OK;
                _makePrimaryResponse.Message = "Vehicle is made to be primary";
                _makePrimaryResponse.Vehicle = vehicleForResponse;
                return _makePrimaryResponse;
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

        ///Additional method to get display names for the method GetVehicleTypes()
        public static string[] GetDisplayNames(Enums.VehicleTypes[] enumValues)
        {
            return enumValues.Select(enumValue =>
            {
                var displayAttribute = enumValue.GetType()
                                                .GetField(enumValue.ToString())
                                                .GetCustomAttributes(typeof(DisplayAttribute), false)
                                                .FirstOrDefault() as DisplayAttribute;

                return displayAttribute?.Name ?? enumValue.ToString();
            }).ToArray();
        }

    }
}