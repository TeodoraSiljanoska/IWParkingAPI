using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AutoMapper;
using ParkingLotRequest = IWParkingAPI.Models.Data.ParkingLotRequest;
using IWParkingAPI.Models.Context;
using static IWParkingAPI.Models.Enums.Enums;
using IWParkingAPI.Models.Data;
using IWParkingAPI.CustomExceptions;
using NLog;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Services.Implementation
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly RequestResponse _response;
        private readonly GetAllParkingLotRequestsResponse _allRequestsResponse;
        private readonly IMapper _mapper;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public RequestService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _response = new RequestResponse();
            _allRequestsResponse = new GetAllParkingLotRequestsResponse();
            _mapper = MapperConfig.InitializeAutomapper();

        }

        public GetAllParkingLotRequestsResponse GetAllRequests()
        {
            try
            {
                var requests = _requestRepository.GetAsQueryable(x => x.Status == (int)Status.Pending, null, x => x.Include(y => y.User).Include(y => y.ParkingLot)).ToList();
                  
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
                    GetAllRequestsDTOList.Add(_mapper.Map<RequestDTO>(p));
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

                var req = _requestRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User).Include(y => y.ParkingLot)).FirstOrDefault();

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

                var parkingLot = req.ParkingLot;

                if (parkingLot == null)
                {
                    throw new NotFoundException("Parking lot not found");
                }

                req.Status = (int)enumValue;
                req.TimeCreated = DateTime.Now;

                parkingLot.Status = (int)enumValue;
                parkingLot.TimeModified = DateTime.Now;

                _requestRepository.Update(req);
                _parkingLotRepository.Update(parkingLot);
                _unitOfWork.Save();

                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request modified successfully";

                var reqDTO = _mapper.Map<RequestDTO>(req);
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
