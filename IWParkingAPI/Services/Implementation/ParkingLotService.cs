using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net;
using static IWParkingAPI.Models.Enums.Enums;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<TempParkingLot> _tempParkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Zone> _zoneRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly AllParkingLotsResponse _getDTOResponse;
        private readonly ParkingLotResponse _response;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJWTDecode _jWTDecode;
        private const int PageSize = 1;
        private const int PageNumber = 10;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _tempParkingLotRepository = _unitOfWork.GetGenericRepository<TempParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _cityRepository = _unitOfWork.GetGenericRepository<City>();
            _zoneRepository = _unitOfWork.GetGenericRepository<Zone>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _response = new ParkingLotResponse();
            _getDTOResponse = new AllParkingLotsResponse();
            _jWTDecode = jWTDecode;
        }
        public AllParkingLotsResponse GetAllParkingLots(int pageNumber, int pageSize, FilterParkingLotRequest request)
        {
            try
            {
                IQueryable<ParkingLot> query = null;
                query = _parkingLotRepository.GetAsQueryable();

                var userId = _jWTDecode.ExtractClaimByType("Id");
                var role = _jWTDecode.ExtractClaimByType("Role");

                if (userId == null || role.Equals(Models.UserRoles.User))
                {
                    query = query.Where(x => x.Status == (int)Status.Approved && x.IsDeactivated == false);
                }
                else if (role.Equals(Models.UserRoles.Owner))
                {
                    query = query.Where(x => x.UserId == int.Parse(userId) && x.Status == (int)Status.Approved);
                }
                else if (role.Equals(Models.UserRoles.SuperAdmin))
                {
                    query = query.Where(x => x.Status == (int)Status.Approved);
                }
                else
                {
                    query = query.Where(x => x.Status == (int)Status.Approved && x.IsDeactivated == false);
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
                if (pageNumber == 0)
                {
                    pageNumber = PageSize;
                }
                if (pageSize == 0)
                {
                    pageSize = PageNumber;
                }

                var paginatedParkingLots = filteredParkingLots.Skip((pageNumber - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToList();

                if (!filteredParkingLots.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "There aren't any parking lots.";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotDTO>();
                    return _getDTOResponse;
                }

                List<ParkingLotDTO> parkingLotDTOs = new List<ParkingLotDTO>();
                foreach (var p in paginatedParkingLots)
                {
                    parkingLotDTOs.Add(_mapper.Map<ParkingLotDTO>(p));
                }
                _getDTOResponse.StatusCode = HttpStatusCode.OK;
                _getDTOResponse.Message = "Parking lots returned successfully";
                _getDTOResponse.ParkingLots = parkingLotDTOs;
                return _getDTOResponse;
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
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
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


                var city = _cityRepository.GetAsQueryable(x => x.Name == request.City).FirstOrDefault();
                if (city == null)
                {
                    throw new BadRequestException("City with that name doesn't exist");
                }

                var zone = _zoneRepository.GetById(1);


                var existingPL = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City.Equals(city.Name), null, null).FirstOrDefault();
                var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City.Equals(city.Name), null, null).FirstOrDefault();
                if (existingPL != null || expl != null)
                {
                    throw new BadRequestException("Parking Lot with that name already exists");
                }

                var existingInPL = _parkingLotRepository.GetAsQueryable(p => p.City.Equals(city.Name) && p.Address == request.Address
                && p.Zone.Equals(zone.Name) && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();


                var existingInTemp = _tempParkingLotRepository.GetAsQueryable(p => p.City.Equals(city.Name) && p.Address == request.Address
                && p.Zone.Equals(zone.Name) && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
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
                parkingLot.WorkingHourTo = from;
                parkingLot.WorkingHourTo = to;
                parkingLot.ParkingLotId = null;
                parkingLot.City = city.Name;
                parkingLot.Zone = "A1";
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
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);

                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId, null, x => x.Include(y => y.Users)).FirstOrDefault();//.Where(x => x.UserId == userId); 

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                if (parkingLot.Name != request.Name)
                {
                    var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City.Equals(request.City), null, null).FirstOrDefault();
                    var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City.Equals(request.City), null, null).FirstOrDefault();
                    if (existingpl != null || expl != null)
                    {
                        throw new BadRequestException("Parking Lot with that name already exists");
                    }
                }

                TimeSpan from;
                TimeSpan to;
                var resFrom = TimeSpan.TryParse(request.WorkingHourFrom, out from);
                var resTo = TimeSpan.TryParse(request.WorkingHourTo, out to);

                var pl = _parkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City.Equals(request.City) && parkingLot.Zone.Equals(request.Zone) &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == from &&
                   parkingLot.WorkingHourTo == to && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();
                var pl1 = _tempParkingLotRepository.GetAsQueryable(parkingLot => parkingLot.Name == request.Name && parkingLot.City.Equals(request.City) && parkingLot.Zone.Equals(request.Zone) &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == from &&
                   parkingLot.WorkingHourTo == to && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price, null, null).FirstOrDefault();

                if (pl != null || pl1 != null)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                var existingplfromuser = _parkingLotRepository.GetAsQueryable(p => p.City.Equals(request.City) && p.Address == request.Address
                && p.Zone.Equals(request.Zone) && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();

                var existingplfromuser1 = _tempParkingLotRepository.GetAsQueryable(p => p.City.Equals(request.City) && p.Address == request.Address
                && p.Zone.Equals(request.Zone) && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();

                if (existingplfromuser != null || existingplfromuser1 != null)
                {
                    throw new BadRequestException("Parking Lot with that specifications already exists");
                }

                var tempParkingLot = _mapper.Map<TempParkingLot>(request);
                tempParkingLot.Status = (int)Status.Pending;
                tempParkingLot.TimeCreated = DateTime.Now;
                tempParkingLot.UserId = parkingLot.UserId;
                tempParkingLot.User = parkingLot.User;
                tempParkingLot.ParkingLotId = parkingLot.Id;
                _tempParkingLotRepository.Insert(tempParkingLot);

                _unitOfWork.Save();

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
                    ParkingLotRequest plrequest = new ParkingLotRequest();

                    plrequest.ParkingLotId = parkingLot.Id;
                    plrequest.UserId = parkingLot.UserId;
                    plrequest.TimeCreated = DateTime.Now;
                    plrequest.Status = (int)Status.Pending;
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
                if (strUserId == null)
                {
                    throw new BadRequestException("Unexpected error while Creating the Parking Lot");
                }
                var userId = Convert.ToInt32(strUserId);
                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id && p.UserId == userId, null, x => x.Include(y => y.Users)).FirstOrDefault();

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
                    plrequest.Status = (int)Status.Pending;
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

        public AllParkingLotsResponse GetUserFavouriteParkingLots()
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

                var userWithParkingLots = _userRepository.GetAsQueryable(x => x.Id == userId, null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                if (!userWithParkingLots.ParkingLotsNavigation.Any())
                {
                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _getDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotDTO>();
                    return _getDTOResponse;
                }

                var favouritesList = userWithParkingLots.ParkingLotsNavigation.ToList();

                var approvedFromFavourites = favouritesList.Where(a => a.Status == (int)Status.Approved && a.IsDeactivated == false).ToList();

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

        /*    public IEnumerable<ParkingLotDTO> FilterParkingLots(FilterParkingLotRequest request)
            {
                try
                {
                    IQueryable<ParkingLot> query = null;

                    query = _parkingLotRepository.GetAsQueryable(x => x.Status == (int)Status.Approved && x.IsDeactivated == false);

                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        query = query.Where(x => x.Name == request.Name);
                    }
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        query = query.Where(x => x.City == request.City);
                    }
                    if (!string.IsNullOrEmpty(request.Zone))
                    {
                        query = query.Where(x => x.Zone == request.Zone);
                    }
                    if (!string.IsNullOrEmpty(request.Address))
                    {
                        query = query.Where(x => x.Address == request.Address);
                    }
                    if (request.CapacityCar != null)
                    {
                        query = query.Where(x => x.CapacityCar >= request.CapacityCar);
                    }
                    if (request.CapacityAdaptedCar != null)
                    {
                        query = query.Where(x => x.CapacityAdaptedCar >= request.CapacityAdaptedCar);
                    }

                    var filteredParkingLots = query.ToList();

                    var filteredParkingLotsDTO = new List<ParkingLotDTO>();
                    foreach (var p in filteredParkingLots)
                    {
                        filteredParkingLotsDTO.Add(_mapper.Map<ParkingLotDTO>(p));
                    }

                    _getDTOResponse.StatusCode = HttpStatusCode.OK;
                    _getDTOResponse.Message = "Filtered Parking Lots returned successfully";
                    _getDTOResponse.ParkingLots = filteredParkingLotsDTO;
                    return _getDTOResponse;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected error while filtering Parking Lots {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                    throw new InternalErrorException("Unexpected error while filtering Parking Lots");
                }
            } */


    }
}
