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

namespace IWParkingAPI.Services.Implementation
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly RequestResponse _response;
        private readonly IMapper _mapper;

        public RequestService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _response = new RequestResponse();
            _mapper = MapperConfig.InitializeAutomapper();
        }
        public RequestResponse ModifyRequest(int id, RequestRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Request Id is required");
                }

                if (request.Status == null || request.Status.Length == 0)
                {
                    throw new BadRequestException("Status is required");
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
            catch (BadRequestException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InternalErrorException("Unexpected error while modifying the Parking lot Request");
            }
        }
    }
}
