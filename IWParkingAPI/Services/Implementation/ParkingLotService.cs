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
using System.Configuration;
using System.Net;
using static IWParkingAPI.Models.Enums.Enums;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<ParkingLotRequest> _parkingLotRequestRepository;
        private readonly IGenericRepository<ApplicationUser> _userRepository;
        private readonly IUnitOfWork<ParkingDbContextCustom> _custom;
        private readonly GetParkingLotsResponse _getResponse;
        private readonly ParkingLotResponse _response;


        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork, IUnitOfWork<ParkingDbContextCustom> custom)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _custom = custom;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _parkingLotRequestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _userRepository = _custom.GetGenericRepository<ApplicationUser>();
            _getResponse = new GetParkingLotsResponse();
            _response = new ParkingLotResponse();
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            var parkingLots = _parkingLotRepository.GetAll();
            if (parkingLots.Count() == 0)
            {
                _getResponse.StatusCode = HttpStatusCode.NoContent;
                _getResponse.Message = "There aren't any parking lots.";
                _getResponse.ParkingLots = Enumerable.Empty<ParkingLot>();
                return _getResponse;
            }
            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Parking lots returned successfully";
            _getResponse.ParkingLots = parkingLots;
            return _getResponse;
        }

        public ParkingLotResponse GetParkingLotById(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Parking Lot Id is required.";
                return _response;

            }

            ParkingLot parkingLot = _parkingLotRepository.GetById(id);

            if (parkingLot == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Parking Lot not found";
                return _response;
            }

            var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);
            _response.ParkingLot = parkingLotDTO;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Parking Lot returned successfully";
            return _response;
        }

        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            ApplicationUser existinguser = _userRepository.GetById(request.UserId);
            if (existinguser == null || existinguser.IsDeactivated == true)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User does not exists.";
                return _response;
            }

            var existingpl = _parkingLotRepository.GetAsQueryable(p => p.Name == request.Name && p.City == request.City, null, null).FirstOrDefault();
            if (existingpl != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "The Parking Lot with that name already exists.";
                return _response;
            }

            if (request.Price <= 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Price should be greater than 0.";
                return _response;
            }

            if (request.CapacityCar <= 0 || request.CapacityAdaptedCar <= 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Capacity should be greater than 0.";
                return _response;
            }

         //   TimeSpan minValue = new TimeSpan(0, 0, 0);
          //  TimeSpan maxValue = new TimeSpan(23, 59, 59);
          //  TimeSpanValidator timeSpanValidator = new TimeSpanValidator(minValue, maxValue);
       //  timeSpanValidator.Validate(request.WorkingHourFrom);

            if (request.WorkingHourFrom.Hours < 0 || request.WorkingHourFrom.Hours > 24 ||
                request.WorkingHourFrom.Minutes < 0 || request.WorkingHourFrom.Minutes > 59 || request.WorkingHourFrom.Seconds < 0 
                || request.WorkingHourFrom.Seconds >59 || request.WorkingHourTo.Hours <0 || request.WorkingHourTo.Hours > 24 
                || request.WorkingHourTo.Minutes <0 || request.WorkingHourTo.Minutes>59 
                ||request.WorkingHourTo.Seconds <0 || request.WorkingHourTo.Seconds >59)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Invalid working hours.";
                return _response;
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
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "An error occured.";
                return _response;
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
    
        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            ParkingLot parkingLot = _parkingLotRepository.GetById(id)
;
            if (parkingLot == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Parking lot not found";
                return _response;
            }

            var parkingLotDTO = _mapper.Map<ParkingLotDTO>(parkingLot);

            if (parkingLot.IsDeactivated == true)
            {
               
                _response.StatusCode = HttpStatusCode.NotModified;
                _response.Message = "Parking lot is already deactivated";
                _response.ParkingLot = parkingLotDTO;
                return _response;
            }

            parkingLot.IsDeactivated = true;
            _parkingLotRepository.Update(parkingLot);
            _unitOfWork.Save();

            _response.ParkingLot = parkingLotDTO;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Parking lot deactivated successfully";

            return _response;
        }

      /*  bool CreateParkingLotRequest(PLRequest request)
        {
            if(request.ParkingLotId == null)
            {
                return false;
            }

            var plrequest = _mapper.Map<ParkingLotRequest>(request);
           plrequest.UserId = request.UserId;
            plrequest.TimeCreated = DateTime.Now;
            plrequest.Status = (int)Status.Pending;
            _parkingLotRequestRepository.Insert(plrequest);
            _unitOfWork.Save();
            return true;
        }
       */
    }
}
