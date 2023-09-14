using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Enums;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static IWParkingAPI.Models.Enums.Enums;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Zone> _zoneRepository;
        private readonly IGenericRepository<TempParkingLot> _tempParkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly AllFavouriteParkingLotsResponse _getDTOResponse;
        private readonly AllParkingLotResponse _allDTOResponse;
        private readonly ParkingLotResponse _response;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICalculateCapacityExtension _calculateCapacityExtension;
        private readonly IEnumsExtension<Enums.VehicleTypes> _enumsExtensionVehicleTypes;
        private readonly IJWTDecode _jWTDecode;
        private const int PageSize = 5;
        private const int PageNumber = 1;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode,
            ICalculateCapacityExtension calculateCapacityExtension, IEnumsExtension<Enums.VehicleTypes> enumsExtension)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _cityRepository = _unitOfWork.GetGenericRepository<City>();
            _zoneRepository = _unitOfWork.GetGenericRepository<Zone>();
            _tempParkingLotRepository = _unitOfWork.GetGenericRepository<TempParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _cityRepository = _unitOfWork.GetGenericRepository<City>();
            _zoneRepository = _unitOfWork.GetGenericRepository<Zone>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _vehicleRepository = _unitOfWork.GetGenericRepository<Vehicle>();
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _calculateCapacityExtension = calculateCapacityExtension;
            _response = new ParkingLotResponse();
            _getDTOResponse = new AllFavouriteParkingLotsResponse();
            _allDTOResponse = new AllParkingLotResponse();
            _jWTDecode = jWTDecode;
            _enumsExtensionVehicleTypes = enumsExtension;
        }

        public AllParkingLotResponse GetAllParkingLots(int pageNumber, int pageSize, FilterParkingLotRequest request)
        {
            try
            {
                IQueryable<ParkingLot> query = null;
                query = _parkingLotRepository.GetAsQueryable();

                var userId = _jWTDecode.ExtractClaimByType("Id");
                var role = _jWTDecode.ExtractClaimByType("Role");

                var userFavouritesList = new List<ParkingLot>();

                if (userId == null)
                {
                    query = query.Where(x => x.IsDeactivated == false);
                }
                else if (role.Equals(Models.UserRoles.User))
                {
                    query = query.Where(x => x.IsDeactivated == false);

                    var userWithParkingLots = _userRepository.GetAsQueryable(x => x.Id == int.Parse(userId),
                        null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                    userFavouritesList = userWithParkingLots.ParkingLotsNavigation.ToList();
                }
                else if (role.Equals(Models.UserRoles.Owner))
                {
                    query = query.Where(x => x.UserId == int.Parse(userId));
                    if (!string.IsNullOrEmpty(request.Status))
                    {
                        ParkingLotStatus enumValue = (ParkingLotStatus)Enum.Parse(typeof(ParkingLotStatus), request.Status);
                        if ((int)enumValue == (int)ParkingLotStatus.Activated)
                            query = query.Where(x => x.IsDeactivated == false);
                        else
                            query = query.Where(x => x.IsDeactivated == true);
                    }
                }
                else if (role.Equals(Models.UserRoles.SuperAdmin))
                {
                    if (!string.IsNullOrEmpty(request.Status))
                    {
                        ParkingLotStatus enumValue = (ParkingLotStatus)Enum.Parse(typeof(ParkingLotStatus), request.Status);
                        if ((int)enumValue == (int)ParkingLotStatus.Activated)
                            query = query.Where(x => x.IsDeactivated == false);
                        else
                            query = query.Where(x => x.IsDeactivated == true);
                    }
                }
                else
                {
                    query = query.Where(x => x.IsDeactivated == false);
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    query = query.Where(x => x.Name.Contains(request.Name));
                }
                if (!string.IsNullOrEmpty(request.City))
                {
                    query = query.Where(x => x.City.Contains(request.City));
                }
                if (!string.IsNullOrEmpty(request.Zone))
                {
                    query = query.Where(x => x.Zone.Contains(request.Zone));
                }
                if (!string.IsNullOrEmpty(request.Address))
                {
                    query = query.Where(x => x.Address.Contains(request.Address));
                }
                if (request.CapacityCar != null)
                {
                    query = query.Where(x => x.CapacityCar >= request.CapacityCar);
                }
                if (request.CapacityAdaptedCar != null)
                {
                    query = query.Where(x => x.CapacityAdaptedCar >= request.CapacityAdaptedCar);
                }


                var filteredParkingLots = query;

                IEnumerable<ParkingLot> paginatedParkingLots = null;
                if (pageNumber == 0)
                {
                    pageNumber = PageNumber;
                }
                if (pageSize == 0)
                {
                    pageSize = PageSize;
                }
                paginatedParkingLots = filteredParkingLots.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var totalCount = filteredParkingLots.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                if (!paginatedParkingLots.Any())
                {
                    _allDTOResponse.StatusCode = HttpStatusCode.OK;
                    _allDTOResponse.Message = "There aren't any parking lots.";
                    _allDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotWithAvailableCapacityDTO>();
                    return _allDTOResponse;
                }

                var date = DateTime.Now;
                TimeSpan parsedTime;
                var resTo = TimeSpan.TryParse(date.TimeOfDay.ToString(), out parsedTime);

                List<ParkingLotWithAvailableCapacityDTO> parkingLotDTOs = new List<ParkingLotWithAvailableCapacityDTO>();
                foreach (var p in paginatedParkingLots)
                {
                    var mappedObject = _mapper.Map<ParkingLotWithAvailableCapacityDTO>(p);

                    if ((role != null && role.Equals(Models.UserRoles.User) || role == null))
                    {
                        if (userFavouritesList.Contains(p))
                        {
                            mappedObject.IsFavourite = true;
                        }
                    }
                    var madeReservationsCar = _calculateCapacityExtension.AvailableCapacity(0, _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.Car), p.Id,
                            date.Date, parsedTime, date.Date, parsedTime);
                    var madeReservationsAdaptedCar = _calculateCapacityExtension.AvailableCapacity(0, _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.AdaptedCar), p.Id,
                        date.Date, parsedTime, date.Date, parsedTime);

                    mappedObject.AvailableCapacityCar = mappedObject.CapacityCar - madeReservationsCar;
                    mappedObject.AvailableCapacityAdaptedCar = mappedObject.CapacityAdaptedCar - madeReservationsAdaptedCar;

                    parkingLotDTOs.Add(mappedObject);
                }
                _allDTOResponse.StatusCode = HttpStatusCode.OK;
                _allDTOResponse.Message = "Parking lots returned successfully";
                _allDTOResponse.ParkingLots = parkingLotDTOs;
                _allDTOResponse.NumPages = totalPages;
                return _allDTOResponse;
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
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new NotFoundException("User not found");
                }
                var userId = Convert.ToInt32(strUserId);

                var existinguser = _userRepository.GetById(userId);
                if (existinguser == null || existinguser.IsDeactivated == true)
                {
                    throw new NotFoundException("User doesn't exist");
                }
                TimeSpan from;
                TimeSpan to;
                var resFrom = TimeSpan.TryParse(request.WorkingHourFrom, out from);
                var resTo = TimeSpan.TryParse(request.WorkingHourTo, out to);
                var city = _cityRepository.GetAsQueryable(c => c.Name == request.City, null, null).FirstOrDefault();
                if (city == null)
                {
                    throw new BadRequestException("City with that name doesn't exist");
                }
                var zone = _zoneRepository.GetAsQueryable(z => z.Name == request.Zone, null, null).FirstOrDefault();
                if (zone == null)
                {
                    throw new BadRequestException("Zone with that name doesn't exist");
                }

                var existingPL = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                if (existingPL != null || expl != null)
                {
                    throw new BadRequestException("Parking Lot with that name already exists");
                }

                var existingInPL = _parkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();


                var existingInTemp = _tempParkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
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
                parkingLot.WorkingHourTo = from;
                parkingLot.WorkingHourTo = to;
                parkingLot.ParkingLotId = null;
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
                plrequest.Status = (int)RequestStatus.Pending;
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
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new NotFoundException("User not found");
                }
                var userId = Convert.ToInt32(strUserId);

                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId && p.IsDeactivated == false, null, x => x.Include(y => y.Users)).FirstOrDefault();//.Where(x => x.UserId == userId); 

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                var city = _cityRepository.GetAsQueryable(c => c.Name == request.City, null, null).FirstOrDefault();
                if (city == null)
                {
                    throw new BadRequestException("City with that name doesn't exist");
                }
                var zone = _zoneRepository.GetAsQueryable(z => z.Name == request.Zone, null, null).FirstOrDefault();
                if (zone == null)
                {
                    throw new BadRequestException("Zone with that name doesn't exist");
                }

                if (parkingLot.Name != request.Name)
                {
                    var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City && p.ParkingLotId != id, null, null).FirstOrDefault();
                    var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                    if (existingpl != null || expl != null)
                    {
                        throw new BadRequestException("Parking Lot with that name already exists");
                    }
                }

                TimeSpan from;
                TimeSpan to;
                var resFrom = TimeSpan.TryParse(request.WorkingHourFrom, out from);
                var resTo = TimeSpan.TryParse(request.WorkingHourTo, out to);

                var pl = _parkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City == request.City && parkingLot.Zone == request.Zone &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == from &&
                   parkingLot.WorkingHourTo == to && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();
                var pl1 = _tempParkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City == request.City && parkingLot.Zone == request.Zone &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == from &&
                   parkingLot.WorkingHourTo == to && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();

                if (pl != null || pl1 != null)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                var existingPLFromUser = _parkingLotRepository.GetAsQueryable(p => p.Id != parkingLot.Id && p.Name != request.Name && p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false, null, null).FirstOrDefault();

                var existingPLFromUser1 = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId != id && p.Name != request.Name && p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false, null, null).FirstOrDefault();

                if (existingPLFromUser != null || existingPLFromUser1 != null)
                {
                    throw new BadRequestException("Parking Lot with that specifications already exists");
                }

                var existingPlFromUser2 = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId == id, null, null).FirstOrDefault();
                if (existingPlFromUser2 != null)
                {
                    existingPlFromUser2.Name = (existingPlFromUser2.Name == request.Name) ? existingPlFromUser2.Name : request.Name;
                    existingPlFromUser2.City = (existingPlFromUser2.City == request.City) ? existingPlFromUser2.City : request.City;
                    existingPlFromUser2.Zone = (existingPlFromUser2.Zone == request.Zone) ? existingPlFromUser2.Zone : request.Zone;
                    existingPlFromUser2.Address = (existingPlFromUser2.Address == request.Address) ? existingPlFromUser2.Address : request.Address;
                    existingPlFromUser2.City = (existingPlFromUser2.City == request.City) ? existingPlFromUser2.City : request.City;
                    existingPlFromUser2.WorkingHourFrom = (existingPlFromUser2.WorkingHourFrom == from) ? existingPlFromUser2.WorkingHourFrom : from;
                    existingPlFromUser2.WorkingHourTo = (existingPlFromUser2.WorkingHourTo == to) ? existingPlFromUser2.WorkingHourTo : to;
                    existingPlFromUser2.CapacityCar = (existingPlFromUser2.CapacityCar == request.CapacityCar) ? existingPlFromUser2.CapacityCar : request.CapacityCar;
                    existingPlFromUser2.CapacityAdaptedCar = (existingPlFromUser2.CapacityAdaptedCar == request.CapacityAdaptedCar) ? existingPlFromUser2.CapacityAdaptedCar : request.CapacityAdaptedCar;
                    existingPlFromUser2.Price = (existingPlFromUser2.Price == request.Price) ? existingPlFromUser2.Price : request.Price;
                    existingPlFromUser2.TimeModified = DateTime.Now;

                    _tempParkingLotRepository.Update(_mapper.Map<TempParkingLot>(existingPlFromUser2));
                }
                else
                {
                    var tempParkingLot = _mapper.Map<TempParkingLot>(request);
                    tempParkingLot.TimeCreated = DateTime.Now;
                    tempParkingLot.UserId = parkingLot.UserId;
                    tempParkingLot.User = parkingLot.User;
                    tempParkingLot.ParkingLotId = parkingLot.Id;
                    _tempParkingLotRepository.Insert(tempParkingLot);

                    _unitOfWork.Save();
                }
                var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == id && x.UserId == parkingLot.UserId, null, null).FirstOrDefault();
                if (existingRequest != null)
                {
                    if (existingRequest.Type == (int)RequestType.Update)
                    {
                        existingRequest.UserId = parkingLot.UserId;
                        existingRequest.Status = (int)RequestStatus.Pending;
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
                    ParkingLotRequest plrequest = new ParkingLotRequest();

                    plrequest.ParkingLotId = parkingLot.Id;
                    plrequest.UserId = parkingLot.UserId;
                    plrequest.TimeCreated = DateTime.Now;
                    plrequest.Status = (int)RequestStatus.Pending;
                    plrequest.Type = (int)RequestType.Update;
                    _parkingLotRequestRepository.Insert(plrequest);
                    _unitOfWork.Save();
                }

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request for updating the Parking Lot created successfully";
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

                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                var strUserRole = _jWTDecode.ExtractClaimByType("Role");
                if (strUserId == null)
                {
                    throw new NotFoundException("User not found");
                }
                var userId = Convert.ToInt32(strUserId);

                ParkingLot parkingLot = new ParkingLot();
                if (strUserRole.Equals(UserRoles.SuperAdmin))
                {
                    parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id, null, x => x.Include(y => y.Users)).FirstOrDefault();
                    if (parkingLot == null)
                    {
                        throw new NotFoundException("Parking Lot not found");
                    }

                    if (parkingLot.IsDeactivated == true)
                    {
                        throw new BadRequestException("Parking Lot is already deactivated");
                    }
                    var request = _requestRepository.GetAsQueryable(x => x.ParkingLotId == id && x.Type == (int)Enums.RequestType.Deactivate, null, null).FirstOrDefault();
                    if (request != null)
                    {
                        _parkingLotRequestRepository.Delete(request);
                        _unitOfWork.Save();
                    }
                    parkingLot.IsDeactivated = true;
                    parkingLot.TimeModified = DateTime.Now;
                    _parkingLotRepository.Update(parkingLot);
                    _unitOfWork.Save();

                    var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
                    _response.ParkingLot = parkingLotDTO;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Message = "Parking Lot deactivated successfully";
                    return _response;
                }
                else
                {
                    parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId,
                        null, x => x.Include(y => y.Users)).FirstOrDefault();
                    if (parkingLot == null)
                    {
                        throw new NotFoundException("Parking Lot not found");
                    }

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
                        plrequest.Status = (int)RequestStatus.Pending;
                        plrequest.Type = (int)RequestType.Deactivate;

                        _parkingLotRequestRepository.Insert(plrequest);
                        _unitOfWork.Save();
                    }
                    var parkingLotDTOResponse = _mapper.Map<ParkingLotDTO>(parkingLot);

                    _response.ParkingLot = parkingLotDTOResponse;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Message = "Request for deactivating the Parking Lot created successfully";

                    return _response;
                }
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

                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);
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

                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);
                var user = _userRepository.GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();
                var parkingLot = _parkingLotRepository.GetById(parkingLotId);
                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                if (parkingLot == null || parkingLot.IsDeactivated == true)
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

        public AllFavouriteParkingLotsResponse GetUserFavouriteParkingLots(int pageNumber, int pageSize)
        {
            try
            {
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);

                var user = _userRepository.GetById(userId);
                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                var userWithParkingLots = _userRepository.
                    GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                if (!userWithParkingLots.ParkingLotsNavigation.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotWithFavouritesDTO>();
                    _getDTOResponse.NumPages = 0;
                    return _getDTOResponse;
                }

                var favouritesList = userWithParkingLots.ParkingLotsNavigation.Where(a => a.IsDeactivated == false).ToList();

                List<ParkingLot> paginatedParkingLots = new List<ParkingLot>();
                if (pageNumber == 0 && pageSize == 0)
                {
                    pageNumber = PageNumber;
                    pageSize = PageSize;
                    paginatedParkingLots = favouritesList.ToList();
                }
                else if (pageNumber == 0)
                {
                    pageNumber = PageNumber;
                    paginatedParkingLots = favouritesList.Skip((pageNumber - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToList();
                }
                else if (pageSize == 0)
                {
                    pageSize = PageSize;
                    paginatedParkingLots = favouritesList.Skip((pageNumber - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToList();
                }
                else
                {
                    paginatedParkingLots = favouritesList.Skip((pageNumber - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToList();
                }


                var totalCount = favouritesList.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                if (!paginatedParkingLots.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotWithFavouritesDTO>();
                    return _getDTOResponse;
                }

                var ParkingLotDTOList = new List<ParkingLotWithFavouritesDTO>();
                foreach (var p in paginatedParkingLots)
                {
                    var mappedObject = _mapper.Map<ParkingLotWithFavouritesDTO>(p);
                    mappedObject.IsFavourite = true;
                    ParkingLotDTOList.Add(mappedObject);
                }

                _getDTOResponse.StatusCode = HttpStatusCode.OK;
                _getDTOResponse.Message = "Favourite parking lots returned successfully";
                _getDTOResponse.ParkingLots = ParkingLotDTOList;
                _getDTOResponse.NumPages = totalPages;
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
