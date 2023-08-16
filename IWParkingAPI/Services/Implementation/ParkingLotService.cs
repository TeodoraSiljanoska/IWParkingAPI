using AutoMapper;
using FluentValidation.Results;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Fluent_Validations.Validators;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using static IWParkingAPI.Models.Enums.Enums;
using ParkingLotRequest = IWParkingAPI.Models.Data.ParkingLotRequest;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly GetParkingLotsResponse _getResponse;
        private readonly GetParkingLotsDTOResponse _getDTOResponse;
        private readonly ParkingLotResponse _response;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _getResponse = new GetParkingLotsResponse();
            _response = new ParkingLotResponse();
            _httpContextAccessor = httpContextAccessor;
            _getDTOResponse = new GetParkingLotsDTOResponse();
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            try
            {
                var parkingLots = _parkingLotRepository.GetAsQueryable(x => x.Status == ((int)Status.Approved)).ToList();
                if (parkingLots.Count() == 0)
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
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while getting the Parking Lot by Id");
            }
        }

        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            try
            {
                if (request == null || request.Name.Length == 0 || request.City.Length == 0 || request.Address.Length == 0 || request.Zone.Length == 0 ||
                    request.WorkingHourFrom == null || request.WorkingHourTo == null || request.CapacityCar == null || request.CapacityAdaptedCar == null ||
                    request.Price == null || request.UserId == null)
                {
                    throw new BadRequestException("Name, City, Address, Zone, WorkingHourFrom, WorkingHourTo, CapacityCar, CapacityAdaptedCar, Price and UserId are required");
                }
                var existinguser = _userRepository.GetById(request.UserId);
                if (existinguser == null || existinguser.IsDeactivated == true)
                {
                    throw new NotFoundException("User doesn't exist");
                }

                var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                if (existingpl != null)
                {
                    throw new BadRequestException("Parking Lot with that name already exists");
                }
             

                var parkingLot = _mapper.Map<ParkingLot>(request);
                parkingLot.UserId = request.UserId;
                parkingLot.TimeCreated = DateTime.Now;
                parkingLot.Status = (int)Status.Pending;
                _parkingLotRepository.Insert(parkingLot);
                _unitOfWork.Save();

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                var createdParkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == parkingLot.Id, null, null).FirstOrDefault();
                if (createdParkingLot == null)
                {
                    throw new InternalErrorException("An error while creating the Parking Lot occurred");
                }

                ParkingLotRequest plrequest = new ParkingLotRequest();

                plrequest.ParkingLotId = parkingLot.Id;
                plrequest.UserId = parkingLot.UserId;
                plrequest.TimeCreated = DateTime.Now;
                plrequest.Status = (int)Status.Pending;
                plrequest.ParkingLot = parkingLot;
                _parkingLotRequestRepository.Insert(plrequest);
                _unitOfWork.Save();


                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Parking Lot created successfully";
                return _response;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (BadRequestException ex)
            {
                throw;
            }
            catch (InternalErrorException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while creating the Parking Lot");
            }
        }


           public ParkingLotResponse UpdateParkingLot(int id, UpdateParkingLotRequest request)
            {
                try
                {
                if (id <= 0 || request == null || request.Name.Length == 0 || request.City.Length == 0 || request.Zone.Length == 0
                    || request.Address.Length == 0 || request.WorkingHourFrom == null || request.WorkingHourTo == null || request.CapacityCar == null || request.CapacityAdaptedCar == null)
                    {
                    throw new BadRequestException("All fields are required");
                    }
                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == id, null, x => x.Include(y => y.Users)).FirstOrDefault();
                
                    if (parkingLot == null)
                    {
                        throw new NotFoundException("Parking Lot not found");
                    }
                int userId = parkingLot.UserId;
                //var result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                // var userId1 = _httpContextAccessor!.HttpContext.User.FindFirstValue("Id");

                if (parkingLot.Name != request.Name)
                {
                    var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
                    if (existingpl != null)
                    {
                        throw new BadRequestException("Parking Lot with that name already exists");
                    }
                }
                if (parkingLot.Name == request.Name && parkingLot.City == request.City && parkingLot.Zone == request.Zone &&
                   parkingLot.Address == request.Address && parkingLot.WorkingHourFrom == request.WorkingHourFrom &&
                   parkingLot.WorkingHourTo == request.WorkingHourTo && parkingLot.CapacityCar == request.CapacityCar &&
                   parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar && parkingLot.Price == request.Price)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }
                var existingplfromuser = _parkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                 && (p.UserId == userId || p.UserId != userId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();
                    if (existingplfromuser != null)
                    {
                        throw new BadRequestException("Parking Lot with that specifications already exists");
                    }

                    if (request.Price <= 0)
                    {
                        throw new BadRequestException("Price should be greater than 0");
                    }

                    if (request.CapacityCar <= 0 || request.CapacityAdaptedCar <= 0)
                    {
                        throw new BadRequestException("Capacity should be greater than 0");
                    }

                if (request.WorkingHourFrom.Hours < 0 || request.WorkingHourFrom.Hours > 24 ||
                request.WorkingHourFrom.Minutes < 0 || request.WorkingHourFrom.Minutes > 59 || request.WorkingHourFrom.Seconds < 0
                || request.WorkingHourFrom.Seconds > 59 || request.WorkingHourTo.Hours < 0 || request.WorkingHourTo.Hours > 24
                || request.WorkingHourTo.Minutes < 0 || request.WorkingHourTo.Minutes > 59
                || request.WorkingHourTo.Seconds < 0 || request.WorkingHourTo.Seconds > 59)
                {
                    throw new BadRequestException("Invalid working hours");
                }

                parkingLot.Name = (parkingLot.Name == request.Name) ? parkingLot.Name : request.Name;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.Zone = (parkingLot.Zone == request.Zone) ? parkingLot.Zone : request.Zone;
                parkingLot.Address = (parkingLot.Address == request.Address) ? parkingLot.Address : request.Address;
                parkingLot.City = (parkingLot.City == request.City) ? parkingLot.City : request.City;
                parkingLot.WorkingHourFrom = (parkingLot.WorkingHourFrom == request.WorkingHourFrom) ?parkingLot.WorkingHourFrom : request.WorkingHourFrom;
                parkingLot.WorkingHourTo = (parkingLot.WorkingHourTo == request.WorkingHourTo) ? parkingLot.WorkingHourTo : request.WorkingHourTo;
                parkingLot.CapacityCar = (parkingLot.CapacityCar == request.CapacityCar) ? parkingLot.CapacityCar : request.CapacityCar;
                parkingLot.CapacityAdaptedCar = (parkingLot.CapacityAdaptedCar == request.CapacityAdaptedCar) ? parkingLot.CapacityAdaptedCar : request.CapacityAdaptedCar;
                parkingLot.Price = (parkingLot.Price == request.Price) ? parkingLot.Price : request.Price;
                parkingLot.UserId = userId;
                parkingLot.TimeModified = DateTime.Now;

                //saveParkingLot.UserId = Convert.ToInt32(userId);
                //saveParkingLot.TimeModified = DateTime.Now;
                parkingLot.Status = (int)Status.Pending;
                _parkingLotRepository.Update(parkingLot);
                _unitOfWork.Save();

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
              
                var existingRequest = _requestRepository.GetAsQueryable(x => x.ParkingLotId == id && x.UserId == parkingLot.UserId, null, null).FirstOrDefault();
                if(existingRequest != null)
                {
                    if (existingRequest.Status == 1)
                    {
                        existingRequest.UserId = parkingLot.UserId;
                        existingRequest.Status = (int)Status.Pending;
                        existingRequest.TimeCreated = DateTime.Now;
                        _requestRepository.Update(existingRequest);
                        _unitOfWork.Save();
                    }
                }

                if (existingRequest == null)
                {

                    ParkingLotRequest plrequest = new ParkingLotRequest();

                    plrequest.ParkingLotId = parkingLot.Id;
                    plrequest.UserId = parkingLot.UserId;
                    plrequest.TimeCreated = DateTime.Now;
                    plrequest.Status = (int)Status.Pending;
                    plrequest.ParkingLot = parkingLot;
                    _parkingLotRequestRepository.Insert(plrequest);
                    _unitOfWork.Save();
                }

                    _response.ParkingLot = parkingLotDTO;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Message = "Parking Lot updated successfully";
                    return _response;
                }
                catch(BadRequestException ex)
                {
                    throw;
                }
                catch(NotFoundException ex)
                {
                    throw;
                }
                catch(InternalErrorException ex)
                {
                    throw new InternalErrorException("Unexpected error while updating the Parking Lot");
                }
            }
        

        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            try
            {
                if(id <= 0)
                {
                    throw new BadRequestException("ParkingLotId is required");
                }
                ParkingLot parkingLot = _parkingLotRepository.GetById(id)
;
                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking Lot not found");
                }

                var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

                if (parkingLot.IsDeactivated == true)
                {
                    throw new BadRequestException("Parking Lot is already deactivated");
                }

                parkingLot.IsDeactivated = true;
                _parkingLotRepository.Update(parkingLot);
                _unitOfWork.Save();

                _response.ParkingLot = parkingLotDTO;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Parking lot deactivated successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while deactivating the Parking Lot");
            }
        }

        public ParkingLotResponse RemoveParkingLotFavourite(int userId, int parkingLotId)
        {
            try
            {
                if (userId <= 0 || parkingLotId <= 0)
                {
                    throw new BadRequestException("UserId and ParkingLotId are required");
                }
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
                throw;
            }
            catch (BadRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while removing the Parking Lot from Favourites");
            }
        }

        public ParkingLotResponse MakeParkingLotFavorite(int userId, int parkingLotId)
        {
            try
            {
                if(userId <= 0 || parkingLotId <= 0)
                {
                    throw new BadRequestException("UserId and ParkingLotId are required");
                }

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
                throw;
            }
            catch (BadRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while adding the Parking Lot Favourites");
            }
        }

        public GetParkingLotsDTOResponse GetUserFavouriteParkingLots(int userId)
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
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while getting all favourite Parking Lots");
            }
        }
    }
}
