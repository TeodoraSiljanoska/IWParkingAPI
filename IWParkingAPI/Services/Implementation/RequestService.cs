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
using static IWParkingAPI.Models.Data.EnumClass;

namespace IWParkingAPI.Services.Implementation
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLotRequest> _requestRepository;
        private readonly RequestResponse _response;
        private readonly IMapper _mapper;

        public RequestService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _requestRepository = _unitOfWork.GetGenericRepository<ParkingLotRequest>();
            _response = new RequestResponse();
            _mapper = MapperConfig.InitializeAutomapper();
        }
        public RequestResponse ModifyRequest(int id, RequestRequest request)
        {
            var req = _requestRepository.GetAsQueryable(x => x.Id == id, null, x => x.Include(y => y.User)).FirstOrDefault();

            if (req == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Request not found";
                return _response;
            }

            if (!Enum.IsDefined(typeof(StatusEnum), request.Status))
            {
                _response.Message = "Status is invalid";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
            StatusEnum enumValue = (StatusEnum)Enum.Parse(typeof(StatusEnum), request.Status);

            req.Status = (int)enumValue;
            req.TimeCreated = DateTime.Now;

            _requestRepository.Update(req);
            _unitOfWork.Save();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Request modified successfully";

            var reqDTO = _mapper.Map<RequestDTO>(req);
            _response.Request = reqDTO;

            return _response;
        }
    }
}
