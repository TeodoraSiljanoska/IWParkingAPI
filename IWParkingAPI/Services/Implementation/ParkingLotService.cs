using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
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
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<Zone> _zoneRepository;
        private readonly IGenericRepository<TempParkingLot> _tempParkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly AllParkingLotResponse _allDTOResponse;
        private readonly ParkingLotResponse _response;
        private readonly ResponseBase _baseResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICalculateCapacityExtension _calculateCapacityExtension;
        private readonly IEnumsExtension<VehicleTypes> _enumsExtensionVehicleTypes;
        private readonly IJWTDecode _jWTDecode;
        private const int PageSize = 5;
        private const int PageNumber = 1;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode,
            ICalculateCapacityExtension calculateCapacityExtension, IEnumsExtension<VehicleTypes> enumsExtension)
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
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _calculateCapacityExtension = calculateCapacityExtension;
            _response = new ParkingLotResponse();
            _allDTOResponse = new AllParkingLotResponse();
            _baseResponse = new ResponseBase();
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

                UserIdAndRoleFilter(request, ref query, userId, role, ref userFavouritesList);
                query = CheckFilterParams(request, query);

                var filteredParkingLots = query;

                IEnumerable<ParkingLot> paginatedParkingLots;
                int totalPages;
                PaginateParkingLots(ref pageNumber, ref pageSize, filteredParkingLots,
                    out paginatedParkingLots, out totalPages);

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
                    ParkingLotWithAvailableCapacityDTO mappedObject =
                        CheckIsFavouriteAndSetAvailableCapacity(role, userFavouritesList, date, parsedTime, p);

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

        public ParkingLotResponse GetParkingLotById(int parkingLotId)
        {
            try
            {
                ParkingLot parkingLot = CheckIfPLExists(parkingLotId);

                var date = DateTime.Now;
                TimeSpan parsedTime;
                var resTo = TimeSpan.TryParse(date.TimeOfDay.ToString(), out parsedTime);

                var parkingLotDTO = _mapper.Map<ParkingLotWithAvailableCapacityDTO>(parkingLot);

                var madeReservationsCar = _calculateCapacityExtension.AvailableCapacity(0,
                    _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.Car), parkingLotId,
                    date.Date, parsedTime, date.Date, parsedTime);
                var madeReservationsAdaptedCar = _calculateCapacityExtension.AvailableCapacity(0,
                    _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.AdaptedCar), parkingLotId,
                    date.Date, parsedTime, date.Date, parsedTime);

                parkingLotDTO.AvailableCapacityCar = parkingLotDTO.CapacityCar - madeReservationsCar;
                parkingLotDTO.AvailableCapacityAdaptedCar = parkingLotDTO.CapacityAdaptedCar - madeReservationsAdaptedCar;

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

        public ResponseBase CreateParkingLot(ParkingLotReq request)
        {
            try
            {
                int userId;
                AspNetUser existingUser;
                CheckIfUserExists(out userId, out existingUser);

                TimeSpan from;
                TimeSpan to;
                var resFrom = TimeSpan.TryParse(request.WorkingHourFrom, out from);
                var resTo = TimeSpan.TryParse(request.WorkingHourTo, out to);
                CheckIfZoneAndCityExist(request);
                CheckIfPLAlreadyExists(request, from, to);

                var parkingLot = _mapper.Map<TempParkingLot>(request);
                parkingLot.UserId = userId;
                parkingLot.User = existingUser;
                parkingLot.TimeCreated = DateTime.Now;
                parkingLot.WorkingHourTo = from;
                parkingLot.WorkingHourTo = to;
                parkingLot.ParkingLotId = null;
                _tempParkingLotRepository.Insert(parkingLot);
                _unitOfWork.Save();

                var createdParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == parkingLot.Id, null, null).FirstOrDefault();
                if (createdParkingLot == null)
                {
                    throw new InternalErrorException("An error while creating the Parking Lot occurred");
                }

                ParkingLotRequest plRequest = new ParkingLotRequest();
                plRequest.ParkingLotId = parkingLot.Id;
                plRequest.UserId = parkingLot.UserId;
                plRequest.TimeCreated = DateTime.Now;
                plRequest.Status = (int)RequestStatus.Pending;
                _parkingLotRequestRepository.Insert(plRequest);
                _unitOfWork.Save();

                _baseResponse.StatusCode = HttpStatusCode.OK;
                _baseResponse.Message = $"Request for creating the Parking Lot {request.Name} created successfully";
                return _baseResponse;
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


        public ResponseBase UpdateParkingLot(int parkingLotId, UpdateParkingLotRequest request)
        {
            try
            {
                int userId;
                AspNetUser existingUser;
                CheckIfUserExists(out userId, out existingUser);

                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == parkingLotId &&
                p.UserId == userId && p.IsDeactivated == false,
                null, x => x.Include(y => y.Users)).FirstOrDefault();

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                CheckIfZoneAndCityExistForUpdate(request);

                if (parkingLot.Name != request.Name)
                {
                    var expl = _tempParkingLotRepository.GetAsQueryable(p => p.Name == request.Name
                    && p.City == request.City && p.ParkingLotId != parkingLotId, null, null).FirstOrDefault();
                    var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name
                    && p.City == request.City, null, null).FirstOrDefault();
                    if (existingpl != null || expl != null)
                    {
                        throw new BadRequestException("Parking Lot with that name already exists");
                    }
                }

                TimeSpan from;
                TimeSpan to;
                var resFrom = TimeSpan.TryParse(request.WorkingHourFrom, out from);
                var resTo = TimeSpan.TryParse(request.WorkingHourTo, out to);

                CheckIfUpdatesWereEnteredForUpdate(request, from, to);
                CheckIfPLAlreadyExistsForUpdate(parkingLotId, request, userId, parkingLot, from, to);
                CheckIfPLAlreadyExists2ForUpdate(parkingLotId, request, parkingLot, from, to);
                InsertPLDetailsInTempPL(parkingLotId, parkingLot);

                _baseResponse.StatusCode = HttpStatusCode.OK;
                _baseResponse.Message = $"Request for updating the Parking Lot {parkingLot.Name} created successfully";
                return _baseResponse;
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


        public ResponseBase DeactivateParkingLot(int parkingLotId)
        {
            try
            {
                if (parkingLotId <= 0)
                {
                    throw new BadRequestException("ParkingLotId is required");
                }

                string strUserRole = CheckIfRoleExists();

                int userId;
                AspNetUser existingUser;
                CheckIfUserExists(out userId, out existingUser);

                ParkingLot parkingLot = new ParkingLot();
                if (strUserRole.Equals(UserRoles.SuperAdmin))
                {
                    return DeactivateParkingLotForSuperAdminRole(parkingLotId, out parkingLot);
                }
                else
                {
                    return DeactivateParkingLotForOwnerRole(parkingLotId, userId, out parkingLot);
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

        public ResponseBase RemoveParkingLotFavourite(int parkingLotId)
        {
            try
            {
                AspNetUser? user = CheckIfUserExistsForFavouritesMethod();

                if (user.ParkingLotsNavigation.Count() == 0)
                {
                    throw new NotFoundException("User doesn't have favourite parking lots");
                }

                ParkingLot? parkingLot = CheckIfPLExists(parkingLotId);

                if (!user.ParkingLotsNavigation.Contains(parkingLot))
                {
                    throw new BadRequestException("Parking Lot isn't in your favourites");
                }

                user.ParkingLotsNavigation.Remove(parkingLot);
                _userRepository.Update(user);
                _unitOfWork.Save();

                _baseResponse.Message = $"Parking Lot {parkingLot.Name} successfully removed from Favourites";
                _baseResponse.StatusCode = HttpStatusCode.OK;
                return _baseResponse;
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

        public ResponseBase MakeParkingLotFavorite(int parkingLotId)
        {
            try
            {
                AspNetUser? user = CheckIfUserExistsForFavouritesMethod();

                ParkingLot? parkingLot = CheckIfPLExists(parkingLotId);

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
                if (user.ParkingLotsNavigation.Contains(parkingLot))
                {
                    throw new BadRequestException("Parking Lot is already favourite");
                }

                user.ParkingLotsNavigation.Add(parkingLot);
                _userRepository.Update(user);
                _unitOfWork.Save();

                _baseResponse.StatusCode = HttpStatusCode.OK;
                _baseResponse.Message = $"Parking Lot {parkingLot.Name} successfully added to Favorites";
                return _baseResponse;
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

        public AllParkingLotResponse GetUserFavouriteParkingLots(int pageNumber, int pageSize)
        {
            try
            {
                AspNetUser? userWithParkingLots = CheckIfUserExistsForFavouritesMethod();

                if (!userWithParkingLots.ParkingLotsNavigation.Any())
                {
                    _allDTOResponse.StatusCode = HttpStatusCode.OK;
                    _allDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _allDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotWithAvailableCapacityDTO>();
                    _allDTOResponse.NumPages = 0;
                    return _allDTOResponse;
                }

                var favouritesList = userWithParkingLots.ParkingLotsNavigation.Where(a => a.IsDeactivated == false).ToList();
                List<ParkingLot> paginatedParkingLots;
                int totalPages;
                PaginateFavouriteParkingLots(ref pageNumber, ref pageSize, favouritesList, out paginatedParkingLots, out totalPages);

                if (!paginatedParkingLots.Any())
                {
                    _allDTOResponse.StatusCode = HttpStatusCode.OK;
                    _allDTOResponse.Message = "User doesn't have any favourite parking lots";
                    _allDTOResponse.ParkingLots = Enumerable.Empty<ParkingLotWithAvailableCapacityDTO>();
                    return _allDTOResponse;
                }

                var date = DateTime.Now;
                TimeSpan parsedTime;
                var resTo = TimeSpan.TryParse(date.TimeOfDay.ToString(), out parsedTime);

                var ParkingLotDTOList = new List<ParkingLotWithAvailableCapacityDTO>();
                foreach (var p in paginatedParkingLots)
                {
                    var mappedObject = _mapper.Map<ParkingLotWithAvailableCapacityDTO>(p);
                    mappedObject.IsFavourite = true;

                    var madeReservationsCar = _calculateCapacityExtension.AvailableCapacity(0, _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.Car), p.Id,
                           date.Date, parsedTime, date.Date, parsedTime);
                    var madeReservationsAdaptedCar = _calculateCapacityExtension.AvailableCapacity(0, _enumsExtensionVehicleTypes.GetDisplayName(VehicleTypes.AdaptedCar), p.Id,
                        date.Date, parsedTime, date.Date, parsedTime);

                    mappedObject.AvailableCapacityCar = mappedObject.CapacityCar - madeReservationsCar;
                    mappedObject.AvailableCapacityAdaptedCar = mappedObject.CapacityAdaptedCar - madeReservationsAdaptedCar;

                    ParkingLotDTOList.Add(mappedObject);
                }

                _allDTOResponse.StatusCode = HttpStatusCode.OK;
                _allDTOResponse.Message = "Favourite parking lots returned successfully";
                _allDTOResponse.ParkingLots = ParkingLotDTOList;
                _allDTOResponse.NumPages = totalPages;
                return _allDTOResponse;
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

        private ParkingLot CheckIfPLExists(int parkingLotId)
        {
            try
            {
                if (parkingLotId <= 0)
                {
                    throw new BadRequestException("Parking Lot Id is required");
                }

                ParkingLot parkingLot = _parkingLotRepository.GetById(parkingLotId);

                if (parkingLot == null || parkingLot.IsDeactivated == true)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                return parkingLot;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfPLExists {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfPLExists {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Parking Lot exists in CheckIfPLExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Parking Lot exists in CheckIfPLExists method");
            }
        }
        private void CheckIfUserExists(out int userId, out AspNetUser existingUser)
        {
            try
            {
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new NotFoundException("User not found");
                }
                userId = Convert.ToInt32(strUserId);
                existingUser = _userRepository.GetById(userId);
                if (existingUser == null || existingUser.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfUserExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if the User exists in CheckIfUserExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if the User exists in CheckIfUserExists method");
            }

        }
        private string CheckIfRoleExists()
        {
            try
            {
                var strUserRole = _jWTDecode.ExtractClaimByType("Role");
                if (strUserRole == null)
                {
                    throw new NotFoundException("Role not found");
                }

                return strUserRole;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfRoleExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Role exists in CheckIfRoleExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Role exists in CheckIfRoleExists method");
            }

        }
        private AspNetUser CheckIfUserExistsForFavouritesMethod()
        {
            try
            {
                var strUserId = _jWTDecode.ExtractClaimByType("Id");
                if (strUserId == null)
                {
                    throw new NotFoundException("User not found");
                }
                var userId = Convert.ToInt32(strUserId);
                var user = _userRepository.GetAsQueryable(x => x.Id == userId,
                    null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                if (user == null || user.IsDeactivated == true)
                {
                    throw new NotFoundException("User not found");
                }

                return user;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfUserExistsForFavouritesMethod {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if User exists in CheckIfUserExistsForFavouritesMethod method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if User exists in CheckIfUserExistsForFavouritesMethod method");
            }

        }
        private void CheckIfPLAlreadyExists(ParkingLotReq request, TimeSpan from, TimeSpan to)
        {
            try
            {
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfPLAlreadyExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Parking Lot already exists in CheckIfPLAlreadyExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Parking Lot already eaxists in CheckIfPLAlreadyExists method");
            }

        }
        private void CheckIfZoneAndCityExist(ParkingLotReq request)
        {
            try
            {
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfZoneAndCityExist {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if City and Zone exists in CheckIfZoneAndCityExist method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if City and Zone exist in CheckIfZoneAndCityExist method");
            }

        }
        private void CheckIfZoneAndCityExistForUpdate(UpdateParkingLotRequest request)
        {
            try
            {
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfZoneAndCityExistForUpdate {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Zone and City exists for CheckIfZoneAndCityExistForUpdate method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Zone and City exist in CheckIfZoneAndCityExistForUpdate method");
            }

        }
        private void CheckIfUpdatesWereEnteredForUpdate(UpdateParkingLotRequest request, TimeSpan from, TimeSpan to)
        {
            try
            {
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfUpdatesWereEnteredForUpdate {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Updates were entered for CheckIfUpdatesWereEnteredForUpdate method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if updates were entered for CheckIfUpdatesWereEnteredForUpdate method");
            }
        }
        private void CheckIfPLAlreadyExistsForUpdate(int parkingLotId, UpdateParkingLotRequest request, int userId, ParkingLot parkingLot, TimeSpan from, TimeSpan to)
        {
            try
            {
                var existingPLFromUser = _parkingLotRepository.GetAsQueryable(p => p.Id != parkingLot.Id
                && p.Name != request.Name && p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false, null, null).FirstOrDefault();

                var existingPLFromUser1 = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId != parkingLotId
                && p.Name != request.Name && p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == from && p.WorkingHourTo == to &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false, null, null).FirstOrDefault();

                if (existingPLFromUser != null || existingPLFromUser1 != null)
                {
                    throw new BadRequestException("Parking Lot with that specifications already exists");
                }
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfPLAlreadyExistsForUpdate {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Parking Lot already exists in CheckIfPLAlreadyExistsForUpdate method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Parking Lot already exists in CheckIfPLAlreadyExistsForUpdate method");
            }
        }
        private void InsertPLDetailsInTempPL(int parkingLotId, ParkingLot parkingLot)
        {
            try
            {
                var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == parkingLotId && x.UserId == parkingLot.UserId, null, null).FirstOrDefault();
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
                else
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfRequestExistsOrElseCreateNew {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Request exists in CheckIfRequestExistsOrElseCreateNew method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Request exists in CheckIfRequestExistsOrElseCreateNew method");
            }

        }
        private ResponseBase DeactivateParkingLotForSuperAdminRole(int parkingLotId, out ParkingLot parkingLot)
        {
            parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == parkingLotId,
            null, x => x.Include(y => y.Users)).FirstOrDefault();
            if (parkingLot == null || parkingLot.IsDeactivated == true)
            {
                throw new NotFoundException("Parking Lot not found");
            }

            var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == parkingLotId &&
            x.Type == (int)RequestType.Deactivate, null, null).FirstOrDefault();
            if (existingRequest != null)
            {
                _parkingLotRequestRepository.Delete(existingRequest);
                _unitOfWork.Save();
            }
            parkingLot.IsDeactivated = true;
            parkingLot.TimeModified = DateTime.Now;
            _parkingLotRepository.Update(parkingLot);
            _unitOfWork.Save();

            _baseResponse.StatusCode = HttpStatusCode.OK;
            _baseResponse.Message = $"Parking Lot {parkingLot.Name} deactivated successfully";
            return _baseResponse;
        }
        private ResponseBase DeactivateParkingLotForOwnerRole(int parkingLotId, int userId, out ParkingLot parkingLot)
        {
            parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == parkingLotId && p.UserId == userId,
            null, x => x.Include(y => y.Users)).FirstOrDefault();
            if (parkingLot == null)
            {
                throw new NotFoundException("Parking Lot not found");
            }

            if (parkingLot.IsDeactivated == true)
            {
                throw new BadRequestException("Parking Lot is already deactivated");
            }
            var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == parkingLotId &&
            x.UserId == userId).FirstOrDefault();
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
            _baseResponse.StatusCode = HttpStatusCode.OK; 
            _baseResponse.Message = $"Request for deactivating the Parking Lot {parkingLot.Name} created successfully"; 
            return _baseResponse;
        }
        private static void PaginateFavouriteParkingLots(ref int pageNumber, ref int pageSize, List<ParkingLot> favouritesList, out List<ParkingLot> paginatedParkingLots, out int totalPages)
        {
            paginatedParkingLots = new List<ParkingLot>();
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
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }
        private void CheckIfPLAlreadyExists2ForUpdate(int parkingLotId, UpdateParkingLotRequest request, ParkingLot parkingLot, TimeSpan from, TimeSpan to)
        {
            var existingPlFromUser2 = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId == parkingLotId).FirstOrDefault();
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
        }

        private ParkingLotWithAvailableCapacityDTO CheckIsFavouriteAndSetAvailableCapacity(string? role, List<ParkingLot> userFavouritesList, DateTime date, TimeSpan parsedTime, ParkingLot p)
        {
            var mappedObject = _mapper.Map<ParkingLotWithAvailableCapacityDTO>(p);

            if ((role != null && role.Equals(UserRoles.User) || role == null))
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
            return mappedObject;
        }
        private void UserIdAndRoleFilter(FilterParkingLotRequest request, ref IQueryable<ParkingLot> query, string userId, string? role, ref List<ParkingLot> userFavouritesList)
        {
            if (userId == null)
            {
                query = query.Where(x => x.IsDeactivated == false);
            }
            else if (role.Equals(UserRoles.User))
            {
                query = query.Where(x => x.IsDeactivated == false);

                var userWithParkingLots = _userRepository.GetAsQueryable(x => x.Id == int.Parse(userId),
                    null, x => x.Include(y => y.ParkingLotsNavigation)).FirstOrDefault();

                userFavouritesList = userWithParkingLots.ParkingLotsNavigation.ToList();
            }
            else if (role.Equals(UserRoles.Owner))
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
            else if (role.Equals(UserRoles.SuperAdmin))
            {
                if (!string.IsNullOrEmpty(request.Status))
                {
                    ParkingLotStatus enumValue = (ParkingLotStatus)Enum.Parse(typeof(ParkingLotStatus), request.Status);
                    if ((int)enumValue == (int)ParkingLotStatus.Activated)
                        query = query.Where(x => x.IsDeactivated == false);
                    else if ((int)enumValue == (int)ParkingLotStatus.Deactivated)
                        query = query.Where(x => x.IsDeactivated == true);
                    else
                        query = query;
                }
            }
            else
            {
                query = query.Where(x => x.IsDeactivated == false);
            }
        }
        
        private static void PaginateParkingLots(ref int pageNumber, ref int pageSize, IQueryable<ParkingLot> filteredParkingLots, out IEnumerable<ParkingLot> paginatedParkingLots, out int totalPages)
        {
            paginatedParkingLots = null;
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
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }

        private static IQueryable<ParkingLot> CheckFilterParams(FilterParkingLotRequest request, IQueryable<ParkingLot> query)
        {
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
            return query;
        }

    }
}
