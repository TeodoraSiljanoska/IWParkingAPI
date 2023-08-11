using AutoMapper;
using IWParkingAPI.CustomExceptions;
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
        private readonly GetParkingLotsResponse _getResponse;
        private readonly ParkingLotResponse _response;


        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _getResponse = new GetParkingLotsResponse();
            _response = new ParkingLotResponse();
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
            catch(BadRequestException ex)
            {
                throw;
            }
            catch(NotFoundException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new InternalErrorException("Unexpected error while deactivating the Parking Lot");
            }
        }

        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            try
            {
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
                var existingplfromuser = _parkingLotRepository.GetAsQueryable(p => p.City == request.City && p.Address == request.Address
                && p.Zone == request.Zone && p.WorkingHourFrom == request.WorkingHourFrom && p.WorkingHourTo == request.WorkingHourTo &&
                p.Price == request.Price && p.CapacityCar == request.CapacityCar && p.CapacityAdaptedCar == request.CapacityAdaptedCar
                && (p.UserId == request.UserId || p.UserId != request.UserId) && p.IsDeactivated == false && p.Name != request.Name, null, null).FirstOrDefault();
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
            catch(NotFoundException ex)
            {
                throw;
            }
            catch(BadRequestException ex)
            {
                throw;
            }
            catch(InternalErrorException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new InternalErrorException("Unexpected error while creating the Parking Lot");
            }
        }

        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            try
            {
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
            catch(NotFoundException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new InternalErrorException("Unexpected error while deactivating the Parking Lot");
            }
        }

        public ParkingLotResponse RemoveParkingLotFavourite(int userId, int parkingLotId)
        {
            try
            {
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
            catch(NotFoundException ex)
            {
                throw;
            }
            catch(BadRequestException ex)
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

    }
}
