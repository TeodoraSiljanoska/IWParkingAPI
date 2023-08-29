using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AutoMapper;
using static IWParkingAPI.Models.Enums.Enums;
using IWParkingAPI.CustomExceptions;
using NLog;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Utilities;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Enums;

namespace IWParkingAPI.Services.Implementation
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<TempParkingLot> _tempParkingLotRepository;
        private readonly RequestResponse _response;
        private readonly AllRequestsResponse _allRequestsResponse;
        private readonly IMapper _mapper;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJWTDecode _jWTDecode;


        public RequestService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode)
        {
            _unitOfWork = unitOfWork;
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _tempParkingLotRepository = _unitOfWork.GetGenericRepository<TempParkingLot>();
            _response = new RequestResponse();
            _allRequestsResponse = new AllRequestsResponse();
            _mapper = MapperConfig.InitializeAutomapper();
            _jWTDecode = jWTDecode;
        }

        public AllRequestsResponse GetAllRequests()
        {
            try
            {
                var userId = _jWTDecode.ExtractClaimByType("Id");

                var role = _jWTDecode.ExtractClaimByType("Role");

                var requests = _requestRepository.GetAsQueryable(x => x.Status == (int)Status.Pending,
                    null, x => x.Include(y => y.User));

                if (role.Equals(Models.UserRoles.Owner))
                {
                    requests = requests.Where(x => x.UserId == int.Parse(userId));
                }

                requests.ToList();

                if (requests.Count() == 0)
                {
                    _allRequestsResponse.StatusCode = HttpStatusCode.OK;
                    _allRequestsResponse.Message = "There aren't any requests.";
                    _allRequestsResponse.Requests = Enumerable.Empty<RequestDTO>();
                    return _allRequestsResponse;
                }

                var GetAllRequestsDTOList = new List<RequestDTO>();
                foreach (var p in requests)
                {
                    var tempPL = _tempParkingLotRepository.GetAsQueryable(x => x.Id == p.ParkingLotId).FirstOrDefault();
                    if (tempPL == null)
                    {
                        throw new BadRequestException("Temporary parking lot id not found");
                    }
                    var plDTO = _mapper.Map<RequestDTO>(p);
                    var tempDTO = _mapper.Map<TempParkingLotDTO>(tempPL);
                    plDTO.ParkingLot = tempDTO;
                    GetAllRequestsDTOList.Add(plDTO);
                }
                _allRequestsResponse.StatusCode = HttpStatusCode.OK;
                _allRequestsResponse.Message = "Requests returned successfully";
                _allRequestsResponse.Requests = GetAllRequestsDTOList;
                return _allRequestsResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Requests {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Requests");
            }
        }
        public RequestResponse ModifyRequest(int id, Models.Requests.RequestRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Request Id is required");
                }

                var req = _requestRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User)).FirstOrDefault();

                if (req == null)
                {
                    throw new NotFoundException("Request not found");
                }

                if (!Enum.IsDefined(typeof(Status), request.Status))
                {
                    throw new NotFoundException("Status not found");
                }

                Status enumValue = (Status)Enum.Parse(typeof(Status), request.Status);

                if (req.Status != (int)Status.Pending)
                {
                    throw new BadRequestException("Request is already approved or declined");
                }

                var parkingLotToDeactivate = _parkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, null).FirstOrDefault();
                var parkingLotToCreateOrUpdate = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, null).FirstOrDefault();

                if (parkingLotToDeactivate == null && parkingLotToCreateOrUpdate == null)
                {
                    throw new NotFoundException("Parking lot not found");
                }

                if ((int)enumValue == (int)Status.Approved && req.Type == (int)RequestType.Activate)
                {
                    var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
                    var tempParkingLotDTO = _mapper.Map<TempParkingLotDTO>(tempParkingLot);
                    var pL = _mapper.Map<ParkingLot>(tempParkingLotDTO);
                    pL.Status = (int)Status.Approved;
                    pL.TimeCreated = DateTime.Now;
                    _parkingLotRepository.Insert(pL);

                    _tempParkingLotRepository.Delete(tempParkingLot);

                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }

                if ((int)enumValue == (int)Status.Declined && req.Type == (int)RequestType.Activate)
                {
                    var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
                    _tempParkingLotRepository.Delete(tempParkingLot);

                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }

                if ((int)enumValue == (int)Status.Approved && req.Type == (int)RequestType.Deactivate)
                {
                    parkingLotToDeactivate.IsDeactivated = true;
                    parkingLotToDeactivate.TimeModified = DateTime.Now;
                    _parkingLotRepository.Update(_mapper.Map<ParkingLot>(parkingLotToDeactivate));
                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }

                if ((int)enumValue == (int)Status.Declined && req.Type == (int)RequestType.Deactivate)
                {
                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }


                if ((int)enumValue == (int)Status.Approved && req.Type == (int)RequestType.Update)
                {
                    var ParkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
                    var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId == ParkingLot.Id && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();


                    ParkingLot.Name = (ParkingLot.Name == tempParkingLot.Name) ? ParkingLot.Name : tempParkingLot.Name;
                    ParkingLot.City = (ParkingLot.City.Equals(tempParkingLot.City)) ? ParkingLot.City : tempParkingLot.City;
                    ParkingLot.Zone = (ParkingLot.Zone.Equals(tempParkingLot.Zone)) ? ParkingLot.Zone : tempParkingLot.Zone;
                    ParkingLot.Address = (ParkingLot.Address == tempParkingLot.Address) ? ParkingLot.Address : tempParkingLot.Address;
                    ParkingLot.City = (ParkingLot.City.Equals(tempParkingLot.City)) ? ParkingLot.City : tempParkingLot.City;
                    ParkingLot.WorkingHourFrom = (ParkingLot.WorkingHourFrom == tempParkingLot.WorkingHourFrom) ? ParkingLot.WorkingHourFrom : tempParkingLot.WorkingHourFrom;
                    ParkingLot.WorkingHourTo = (ParkingLot.WorkingHourTo == tempParkingLot.WorkingHourTo) ? ParkingLot.WorkingHourTo : tempParkingLot.WorkingHourTo;
                    ParkingLot.CapacityCar = (ParkingLot.CapacityCar == tempParkingLot.CapacityCar) ? ParkingLot.CapacityCar : tempParkingLot.CapacityCar;
                    ParkingLot.CapacityAdaptedCar = (ParkingLot.CapacityAdaptedCar == tempParkingLot.CapacityAdaptedCar) ? ParkingLot.CapacityAdaptedCar : tempParkingLot.CapacityAdaptedCar;
                    ParkingLot.Price = (ParkingLot.Price == tempParkingLot.Price) ? ParkingLot.Price : tempParkingLot.Price;
                    ParkingLot.TimeModified = DateTime.Now;


                    ParkingLot.Status = (int)Status.Approved;
                    _parkingLotRepository.Update(_mapper.Map<ParkingLot>(ParkingLot));
                    _tempParkingLotRepository.Delete(tempParkingLot);

                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }

                if ((int)enumValue == (int)Status.Declined && req.Type == (int)RequestType.Update)
                {
                    var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
                    _tempParkingLotRepository.Delete(tempParkingLot);

                    _requestRepository.Delete(req);
                    _unitOfWork.Save();
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request modified successfully";


                var reqDTO = _mapper.Map<RequestDTO>(req);
                reqDTO.Status = (int)enumValue;
                _response.Request = reqDTO;

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for ModifyRequest {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for ModifyRequest {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while modifying the Parking Lot Request {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while modifying the Parking Lot Request");
            }
        }
    }
}
