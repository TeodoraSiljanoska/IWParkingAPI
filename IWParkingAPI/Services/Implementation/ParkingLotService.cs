﻿using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net;
using static IWParkingAPI.Models.Enums.Enums;
//using ParkingLotRequest = IWParkingAPI.Models.Data.ParkingLotRequest;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<TempParkingLot> _tempParkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly GetParkingLotsResponse _getResponse;
        private readonly GetParkingLotsDTOResponse _getDTOResponse;
        private readonly ParkingLotResponse _response;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJWTDecode _jWTDecode;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IHttpContextAccessor httpContextAccessor, IJWTDecode jWTDecode)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _tempParkingLotRepository = _unitOfWork.GetGenericRepository<TempParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _getResponse = new GetParkingLotsResponse();
            _response = new ParkingLotResponse();
            _httpContextAccessor = httpContextAccessor;
            _getDTOResponse = new GetParkingLotsDTOResponse();
            _jWTDecode = jWTDecode;
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                var role = _jWTDecode.ExtractRoleFromToken();

                List<ParkingLot> parkingLots;
                if (userId == 0 || role.Equals(UserRoles.User))
                {
                    parkingLots = _parkingLotRepository.GetAsQueryable(x => x.Status == ((int)Status.Approved)).ToList();
                }
                else if (role.Equals(UserRoles.Owner))
                {
                    parkingLots = _parkingLotRepository.GetAsQueryable(x => x.UserId == userId).ToList();
                }
                else if (role.Equals(UserRoles.SuperAdmin))
                {
                    parkingLots = _parkingLotRepository.GetAsQueryable().ToList();
                }
                else
                {
                    parkingLots = _parkingLotRepository.GetAsQueryable(x => x.Status == ((int)Status.Approved)).ToList();
                }

                if (!parkingLots.Any())
                {
                    _getResponse.StatusCode = HttpStatusCode.OK;
                    _getResponse.Message = "There aren't any parking lots.";
                    _getResponse.ParkingLots = Enumerable.Empty<ParkingLot>();
                    return _getResponse;
                }
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Parking lots returned successfully";
                _getResponse.ParkingLots = parkingLots;
                return _getResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Parking Lots {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Parking Lots");
            }
        }

        public ParkingLotResponse GetParkingLotById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Parking Lot Id is required");
                }

                ParkingLot parkingLot = _parkingLotRepository.GetById(id);

                if (parkingLot == null || parkingLot.IsDeactivated == true)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Parking Lot returned successfully";
                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetParkingLotById {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetParkingLotById {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting the Parking Lot by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Parking Lot by Id");
            }
        }

        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                var existinguser = _userRepository.GetById(userId);
                if (existinguser == null || existinguser.IsDeactivated == true)
                {
                    throw new NotFoundException("User doesn't exist");
                }

                var existingPL = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                if (existingPL != null || expl != null)
                {
                    throw new BadRequestException("Parking Lot with that name already exists");
                }

                var existingInPL = _parkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();

                var existingInTemp = _tempParkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();
                if (existingInPL != null || existingInTemp != null)
                {
                    throw new BadRequestException("Parking Lot with that specifications already exists");
                }


                var parkingLot = _mapper.Map<TempParkingLot>(request);
                parkingLot.UserId = userId;
                parkingLot.User = existinguser;
                parkingLot.TimeCreated = DateTime.Now;
                parkingLot.Status = (int)Status.Pending;
                _tempParkingLotRepository.Insert(parkingLot);
                _unitOfWork.Save();

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                var createdParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == parkingLot.Id, null, null).FirstOrDefault();
                if (createdParkingLot == null)
                {
                    throw new InternalErrorException("An error while creating the Parking Lot occurred");
                }

                ParkingLotRequest plrequest = new ParkingLotRequest();

                plrequest.ParkingLotId = parkingLot.Id;
                plrequest.UserId = parkingLot.UserId;
                plrequest.TimeCreated = DateTime.Now;
                plrequest.Status = (int)Status.Pending;
                //plrequest.ParkingLot = parkingLot;
                _parkingLotRequestRepository.Insert(plrequest);
                _unitOfWork.Save();


                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request for creating the Parking Lot created successfully";
                return _response;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CreateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CreateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for CreateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while creating the Parking Lot {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while creating the Parking Lot");
            }
        }


        public ParkingLotResponse UpdateParkingLot(int id, UpdateParkingLotRequest request)
        {
            try
            {
                int userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                if (id <= 0)
                {
                    throw new BadRequestException("Id is required");
                }

                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId, null, x => x.Include(y => y.Users)).FirstOrDefault();//.Where(x => x.UserId == userId); 

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }
              
              /*  if(parkingLot.UserId != userId)
                {
                    throw new BadRequestException("You don't have permission to update the Parking Lot");
                }
               */
                if (parkingLot.Name != request.Name)
                {
                    var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                    var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                    if (existingpl != null || expl != null)
                    {
                        throw new BadRequestException("Parking Lot with that name already exists");
                    }
                }
                var pl = _parkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City == request.City && parkingLot.Zone == request.Zone &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == request.WorkingHourFrom &&
                   parkingLot.WorkingHourTo == request.WorkingHourTo && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();
                var pl1 = _tempParkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City == request.City && parkingLot.Zone == request.Zone &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == request.WorkingHourFrom &&
                   parkingLot.WorkingHourTo == request.WorkingHourTo && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();

                if (pl != null || pl1 != null)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }
                var existingplfromuser = _parkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();

                var existingplfromuser1 = _tempParkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();
                if (existingplfromuser != null || existingplfromuser1 !=null)
                {
                    throw new BadRequestException("Parking Lot with that specifications already exists");
                }


              /*  parkingLot.Name = (parkingLot.Name == request.Name) ? parkingLot.Name : request.Name;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.Zone = (parkingLot.Zone == request.Zone) ? parkingLot.Zone : request.Zone;
                parkingLot.Address = (parkingLot.Address == request.Address) ? parkingLot.Address : request.Address;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.WorkingHourFrom = (parkingLot.WorkingHourFrom == request.WorkingHourFrom) ? parkingLot.WorkingHourFrom : request.WorkingHourFrom;
                parkingLot.WorkingHourTo = (parkingLot.WorkingHourTo == request.WorkingHourTo) ? parkingLot.WorkingHourTo : request.WorkingHourTo;
                parkingLot.CapacityCar = (parkingLot.CapacityCar == request.CapacityCar) ? parkingLot.CapacityCar : request.CapacityCar;
                parkingLot.CapacityAdaptedCar = (parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar) ? parkingLot.CapacityAdaptedCar : request.CapacityAdaptedCar;
                parkingLot.Price = (parkingLot.Price == request.Price) ? parkingLot.Price : request.Price;
                parkingLot.UserId = userId;
                parkingLot.User = _userRepository.GetAsQueryable(u => u.Id == userId, null, null).FirstOrDefault();
                parkingLot.TimeModified = DateTime.Now;

                parkingLot.Status = (int)Status.Pending; */

                var tempParkingLot = _mapper.Map<TempParkingLot>(request);
                tempParkingLot.Status = (int)Status.Pending;
                tempParkingLot.TimeCreated = DateTime.Now;
                tempParkingLot.UserId = parkingLot.UserId;
                tempParkingLot.User = parkingLot.User;
                tempParkingLot.ParkingLotId = parkingLot.Id;
                _tempParkingLotRepository.Insert(tempParkingLot);
                _unitOfWork.Save();

                /*       parkingLot.Name = (parkingLot.Name == request.Name) ? parkingLot.Name : request.Name;
                       parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                       parkingLot.Zone = (parkingLot.Zone == request.Zone) ? parkingLot.Zone : request.Zone;
                       parkingLot.Address = (parkingLot.Address == request.Address) ? parkingLot.Address : request.Address;
                       parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                       parkingLot.WorkingHourFrom = (parkingLot.WorkingHourFrom == request.WorkingHourFrom) ? parkingLot.WorkingHourFrom : request.WorkingHourFrom;
                       parkingLot.WorkingHourTo = (parkingLot.WorkingHourTo == request.WorkingHourTo) ? parkingLot.WorkingHourTo : request.WorkingHourTo;
                       parkingLot.CapacityCar = (parkingLot.CapacityCar == request.CapacityCar) ? parkingLot.CapacityCar : request.CapacityCar;
                       parkingLot.CapacityAdaptedCar = (parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar) ? parkingLot.CapacityAdaptedCar : request.CapacityAdaptedCar;
                       parkingLot.Price = (parkingLot.Price == request.Price) ? parkingLot.Price : request.Price;
                       parkingLot.UserId = userId;
                       parkingLot.User = _userRepository.GetAsQueryable(u => u.Id == userId, null, null).FirstOrDefault();
                       parkingLot.TimeModified = DateTime.Now;

                       parkingLot.Status = (int)Status.Pending;
                       _parkingLotRepository.Update(parkingLot);
                       _unitOfWork.Save();

                       var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot); */

                var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == id && x.UserId == parkingLot.UserId, null, null).FirstOrDefault();
                if (existingRequest != null)
                {
                    if (existingRequest.Type == (int)RequestType.Update)
                    {
                        existingRequest.UserId = parkingLot.UserId;
                        existingRequest.Status = (int)Status.Pending;
                        existingRequest.Type = (int)RequestType.Update;
                        existingRequest.TimeCreated = DateTime.Now;
                        _requestRepository.Update(existingRequest);
                        _unitOfWork.Save();
                    }

                    if (existingRequest.Type != (int)RequestType.Update)
                    {
                        throw new BadRequestException("There is already a request for this Parking Lot. Please wait until it is processed");
                    }

                }

                if (existingRequest == null)
                {
                  // parkingLot = _mapper.Map<TempParkingLot>(request);

                    ParkingLotRequest plrequest = new ParkingLotRequest();

                    plrequest.ParkingLotId = parkingLot.Id;
                    plrequest.UserId = parkingLot.UserId;
                    plrequest.TimeCreated = DateTime.Now;
                    plrequest.Status = (int)Status.Pending;
                    plrequest.Type = (int)RequestType.Update;
                  //  plrequest.ParkingLot = tempParkingLot;
                    _parkingLotRequestRepository.Insert(plrequest);
                    _unitOfWork.Save();
                }


           /*     parkingLot.Name = (parkingLot.Name == request.Name) ? parkingLot.Name : request.Name;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.Zone = (parkingLot.Zone == request.Zone) ? parkingLot.Zone : request.Zone;
                parkingLot.Address = (parkingLot.Address == request.Address) ? parkingLot.Address : request.Address;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.WorkingHourFrom = (parkingLot.WorkingHourFrom == request.WorkingHourFrom) ? parkingLot.WorkingHourFrom : request.WorkingHourFrom;
                parkingLot.WorkingHourTo = (parkingLot.WorkingHourTo == request.WorkingHourTo) ? parkingLot.WorkingHourTo : request.WorkingHourTo;
                parkingLot.CapacityCar = (parkingLot.CapacityCar == request.CapacityCar) ? parkingLot.CapacityCar : request.CapacityCar;
                parkingLot.CapacityAdaptedCar = (parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar) ? parkingLot.CapacityAdaptedCar : request.CapacityAdaptedCar;
                parkingLot.Price = (parkingLot.Price == request.Price) ? parkingLot.Price : request.Price;
                parkingLot.UserId = userId;
                parkingLot.User = _userRepository.GetAsQueryable(u => u.Id == userId, null, null).FirstOrDefault();
                parkingLot.TimeModified = DateTime.Now;

                parkingLot.Status = (int)Status.Pending;
                _parkingLotRepository.Update(parkingLot);
                _unitOfWork.Save(); */

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Parking Lot updated successfully";
                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for UpdateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for UpdateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Unexpected error while updating the Parking Lot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw new InternalErrorException("Unexpected error while updating the Parking Lot");
            }
        }


        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("ParkingLotId is required");
                }

                int userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId, null, x => x.Include(y => y.Users)).FirstOrDefault();

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }
                
               // int userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
             /*   if (parkingLot.UserId != userId)
                {
                    throw new BadRequestException("You don't have permission to deactivate the Parking Lot");
                }*/

              //  var parkingLotDTO = _mapper.Map<TempParkingLotDTO>(parkingLot);
              //  var tempParkingLot = _mapper.Map<TempParkingLot>(parkingLotDTO);
               // var tempParkingLotDTO = _mapper.Map<TempParkingLotDTO>(tempParkingLot);

                if (parkingLot.IsDeactivated == true)
                {
                    throw new BadRequestException("Parking Lot is already deactivated");
                }
                
                var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == id && x.UserId == parkingLot.UserId, null, null).FirstOrDefault();
                if (existingRequest != null)
                {
                    throw new BadRequestException("There is already a request for this Parking Lot. Please wait until it is processed");
                }

                if (existingRequest == null)
                {

                    ParkingLotRequest plrequest = new ParkingLotRequest();

                    plrequest.ParkingLotId = parkingLot.Id;
                    plrequest.UserId = parkingLot.UserId;
                    plrequest.TimeCreated = DateTime.Now;
                    plrequest.Status = (int)Status.Pending;
                    plrequest.Type = (int)RequestType.Deactivate;
                   // plrequest.ParkingLot = null;
                   // plrequest.ParkingLot = tempParkingLot;
                  //  plrequest.ParkingLot.Id = parkingLot.Id;

                    _parkingLotRequestRepository.Insert(plrequest);
                    //var tempParkingLotDTO = _mapper.Map<ParkingLotDTO>(tempParkingLot);
                   // _tempParkingLotRepository.Insert(_mapper.Map<TempParkingLot>(tempParkingLotDTO));
                    _unitOfWork.Save();
                }
                 var parkingLotDTOResponse = _mapper.Map<ParkingLotDTO>(parkingLot);

                _response.ParkingLot = parkingLotDTOResponse;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request for deactivating the Parking Lot created successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for DeactivateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for DeactivateParkingLot {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while deactivating the Parking Lot {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while deactivating the Parking Lot");
            }
        }

        public ParkingLotResponse RemoveParkingLotFavourite(int parkingLotId)
        {
            try
            {
                if (parkingLotId <= 0)
                {
                    throw new BadRequestException("ParkingLotId is required");
                }

                int userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                var user = _userRepository.GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                if (user.ParkingLotsNavigation.Count() == 0)
                {
                    throw new NotFoundException("User doesn't have favourite parking lots");
                }

                var parkingLot = _parkingLotRepository.GetById(parkingLotId);

                if (parkingLot == null || parkingLot.IsDeactivated == true)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                if (!user.ParkingLotsNavigation.Contains(parkingLot))
                {
                    throw new BadRequestException("Parking Lot isn't in your favourites");
                }

                user.ParkingLotsNavigation.Remove(parkingLot);
                _userRepository.Update(user);
                _unitOfWork.Save();

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
                _response.Message = "Parking Lot successfully removed from favourites.";
                _response.StatusCode = HttpStatusCode.OK;
                _response.ParkingLot = parkingLotDTO;
                return _response;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for RemoveParkingLotFavourite {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for RemoveParkingLotFavourite {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while removing the Parking Lot from Favourites {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while removing the Parking Lot from Favourites");
            }
        }

        public ParkingLotResponse MakeParkingLotFavorite(int parkingLotId)
        {
            try
            {
                if (parkingLotId <= 0)
                {
                    throw new BadRequestException("UserId and ParkingLotId are required");
                }
                
                int userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
                var user = _userRepository.GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();
                var parkingLot = _parkingLotRepository.GetById(parkingLotId);
                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                if (parkingLot == null || parkingLot.IsDeactivated == true || parkingLot.Status != (int)Status.Approved)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                if (user.ParkingLotsNavigation.Contains(parkingLot))
                {
                    throw new BadRequestException("Parking Lot is already favourite");
                }

                user.ParkingLotsNavigation.Add(parkingLot);
                _userRepository.Update(user);
                _unitOfWork.Save();

                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Parking Lot added to Favorites";
                return _response;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for MakeParkingLotFavorite {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for MakeParkingLotFavorite {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while adding the Parking Lot Favourites {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while adding the Parking Lot Favourites");
            }
        }

        public GetParkingLotsDTOResponse GetUserFavouriteParkingLots()
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());

                var user = _userRepository.GetById(userId);
                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                var userWithParkingLots = _userRepository.GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                if (!userWithParkingLots.ParkingLotsNavigation.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotDTO>();
                    return _getDTOResponse;
                }

                var favouritesList = userWithParkingLots.ParkingLotsNavigation.ToList();

                var approvedFromFavourites = favouritesList.Where(a => a.Status == (int)Status.Approved).ToList();

                if (!approvedFromFavourites.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotDTO>();
                    return _getDTOResponse;
                }

                var ParkingLotDTOList = new List<ParkingLotDTO>();
                foreach (var p in approvedFromFavourites)
                {
                    ParkingLotDTOList.Add(_mapper.Map<ParkingLotDTO>(p));
                }

                _getDTOResponse.StatusCode = HttpStatusCode.OK;
                _getDTOResponse.Message = "Favourite parking lots returned successfully";
                _getDTOResponse.ParkingLots = ParkingLotDTOList;
                return _getDTOResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetUserFavouriteParkingLots {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetUserFavouriteParkingLots {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all favourite Parking Lots {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all favourite Parking Lots");
            }
        }
    }
}
