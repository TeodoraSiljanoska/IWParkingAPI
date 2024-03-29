﻿using IWParkingAPI.Infrastructure.Repository;
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
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;

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
        private const int PageSize = 5;
        private const int PageNumber = 1;

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

        public AllRequestsResponse GetAllRequests(int pageNumber, int pageSize)
        {
            try
            {
                string userId, role;
                CheckIfUserIdAndRoleExist(out userId, out role);

                var requests = _requestRepository.GetAsQueryable(x => x.Status == (int)RequestStatus.Pending,
                    null, x => x.Include(y => y.User));

                if (role.Equals(UserRoles.Owner))
                {
                    requests = requests.Where(x => x.UserId == int.Parse(userId));
                }

                int totalPages;
                List<ParkingLotRequest> paginatedRequests;
                PaginateRequests(ref pageNumber, ref pageSize, requests, out totalPages, out paginatedRequests);

                if (paginatedRequests.Count() == 0)
                {
                    _allRequestsResponse.StatusCode = HttpStatusCode.OK;
                    _allRequestsResponse.Message = "There aren't any requests.";
                    _allRequestsResponse.Requests = Enumerable.Empty<RequestDTO>();
                    return _allRequestsResponse;
                }

                var GetAllRequestsDTOList = new List<RequestDTO>();
                foreach (var p in paginatedRequests)
                {
                    RequestDTO plRequestDTO;
                    plRequestDTO = CheckRequestTypeAndMapObject(p);

                    GetAllRequestsDTOList.Add(plRequestDTO);
                }

                _allRequestsResponse.StatusCode = HttpStatusCode.OK;
                _allRequestsResponse.Message = "Requests returned successfully";
                _allRequestsResponse.Requests = GetAllRequestsDTOList;
                _allRequestsResponse.NumPages = totalPages;
                return _allRequestsResponse;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetAllRequests {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Requests {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Requests");
            }
        }

        public RequestResponse ModifyRequest(int requestId, Models.Requests.RequestRequest request)
        {
            try
            {
                ParkingLotRequest? req = CheckIfRequestExists(requestId);

                RequestStatus enumValue = CheckIfRequestStatusIsValid(request);

                var parkingLotToDeactivate = _parkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, null).FirstOrDefault();
                var parkingLotToCreate = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId && p.UserId == req.UserId, null, null).FirstOrDefault();
                var parkingLotToUpdate = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId == req.ParkingLotId && p.UserId == req.UserId, null, null).FirstOrDefault();
                RequestDTO reqDTO = new RequestDTO();

                switch ((RequestStatus)enumValue)
                {
                    case RequestStatus.Approved:
                        switch ((RequestType)req.Type)
                        {
                            case RequestType.Activate:
                                reqDTO = ApproveActivateType(req);
                                break;

                            case RequestType.Deactivate:
                                reqDTO = ApproveDeactivateType(req, parkingLotToDeactivate);
                                break;

                            case RequestType.Update:
                                reqDTO = ApproveUpdateType(req);
                                break;
                        }
                        break;

                    case RequestStatus.Declined:
                        switch ((RequestType)req.Type)
                        {
                            case RequestType.Activate:
                                reqDTO = DeclineActivateType(req);
                                break;

                            case RequestType.Deactivate:
                                reqDTO = DeclineDeactivateType(req, parkingLotToDeactivate);
                                break;

                            case RequestType.Update:
                                reqDTO = DeclineUpdateType(req);
                                break;
                        }
                        break;
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Request modified successfully";

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

        private void CheckIfUserIdAndRoleExist(out string userId, out string role)
        {
            try
            {
                userId = _jWTDecode.ExtractClaimByType("Id");
                role = _jWTDecode.ExtractClaimByType("Role");
                if (userId == null)
                {
                    throw new NotFoundException("User not found");
                }
                if (role == null)
                {
                    throw new NotFoundException("Role not found");
                }
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfUserIdAndRoleExist {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if User Id and User Role exist in CheckIfUserIdAndRoleExist method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if User Id and User Role exist in CheckIfUserIdAndRoleExist method");
            }
        }

        private ParkingLotRequest CheckIfRequestExists(int requestId)
        {
            try
            {
                if (requestId <= 0)
                {
                    throw new BadRequestException("Request Id is required");
                }

                var req = _requestRepository.GetAsQueryable(x => x.Id == requestId,
                    null, x => x.Include(y => y.User)).FirstOrDefault();

                if (req == null)
                {
                    throw new NotFoundException("Request not found");
                }

                if (req.Status != (int)RequestStatus.Pending)
                {
                    throw new BadRequestException("Request is already approved or declined");
                }

                return req;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfRequestExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfRequestExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Request exists in CheckIfRequestExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Request exists in CheckIfRequestExists method");
            }

        }

        private static RequestStatus CheckIfRequestStatusIsValid(RequestRequest request)
        {
            try
            {
                if (!Enum.IsDefined(typeof(RequestStatus), request.Status))
                {
                    throw new NotFoundException("Status not found");
                }

                RequestStatus enumValue = (RequestStatus)Enum.Parse(typeof(RequestStatus), request.Status);
                return enumValue;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfRequestStatusIsValid {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Request is valid in CheckIfRequestStatusIsValid method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Request is valid in CheckIfRequestStatusIsValid method");
            }

        }

        private RequestDTO CheckRequestTypeAndMapObject(ParkingLotRequest plRequest)
        {
            RequestDTO plRequestDTO;
            switch (plRequest.Type)
            {
                case (int)RequestType.Update:
                    {
                        var oldPl = _parkingLotRepository.GetAsQueryable(x => x.Id == plRequest.ParkingLotId).FirstOrDefault();
                        var pl = _tempParkingLotRepository.GetAsQueryable(x => x.ParkingLotId == plRequest.ParkingLotId).FirstOrDefault();
                        var tempDTO = _mapper.Map<TempParkingLotDTO>(pl);
                        var oldDTO = _mapper.Map<TempParkingLotDTO>(oldPl);
                        plRequestDTO = _mapper.Map<RequestDTO>(plRequest);
                        plRequestDTO.ParkingLot = tempDTO;
                        plRequestDTO.OldParkingLot = oldDTO;
                        break;
                    }

                case (int)RequestType.Deactivate:
                    {
                        var pl = _parkingLotRepository.GetAsQueryable(x => x.Id == plRequest.ParkingLotId).FirstOrDefault();
                        var tempDTO = _mapper.Map<TempParkingLotDTO>(pl);
                        plRequestDTO = _mapper.Map<RequestDTO>(plRequest);
                        plRequestDTO.ParkingLot = tempDTO;
                        break;
                    }

                default:
                    {
                        var tempPL = _tempParkingLotRepository.GetAsQueryable(x => x.Id == plRequest.ParkingLotId).FirstOrDefault();
                        var tempDTO = _mapper.Map<TempParkingLotDTO>(tempPL);
                        plRequestDTO = _mapper.Map<RequestDTO>(plRequest);
                        plRequestDTO.ParkingLot = tempDTO;
                        break;
                    }
            }

            if(Enum.TryParse<RequestType>(plRequest.Type.ToString(), out var requestType))
            {
                plRequestDTO.Type = requestType.ToString();
            }
            
            return plRequestDTO;
        }

        private static void PaginateRequests(ref int pageNumber, ref int pageSize, IQueryable<ParkingLotRequest> requests,
            out int totalPages, out List<ParkingLotRequest> paginatedRequests)
        {
            if (pageNumber == 0)
            {
                pageNumber = PageNumber;
            }
            if (pageSize == 0)
            {
                pageSize = PageSize;
            }

            var totalCount = requests.Count();
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            paginatedRequests = requests.Skip((pageNumber - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .ToList();
        }

        private RequestDTO ApproveActivateType(ParkingLotRequest req)
        {
            RequestDTO reqDTO;
            var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId
            && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
            var tempParkingLotDTO = _mapper.Map<TempParkingLotDTO>(tempParkingLot);

            var pL = _mapper.Map<ParkingLot>(tempParkingLotDTO);
            pL.TimeCreated = DateTime.Now;
            _parkingLotRepository.Insert(pL);

            _tempParkingLotRepository.Delete(tempParkingLot);

            _requestRepository.Delete(req);
            _unitOfWork.Save();

            var pl = _parkingLotRepository.GetAsQueryable(p => p.Id == pL.Id && p.UserId == req.UserId,
                null, x => x.Include(y => y.User)).FirstOrDefault();

            var tempDTO = _mapper.Map<TempParkingLotDTO>(pl);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = tempDTO;
            reqDTO.ParkingLot.TimeCreated = DateTime.Now;
            return reqDTO;
        }

        private RequestDTO ApproveUpdateType(ParkingLotRequest req)
        {
            RequestDTO reqDTO;
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

            _parkingLotRepository.Update(_mapper.Map<ParkingLot>(ParkingLot));
            _tempParkingLotRepository.Delete(tempParkingLot);

            _requestRepository.Delete(req);
            _unitOfWork.Save();

            var tempDTO = _mapper.Map<TempParkingLotDTO>(tempParkingLot);
            var oldDTO = _mapper.Map<TempParkingLotDTO>(ParkingLot);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = tempDTO;
            reqDTO.ParkingLot.TimeCreated = ParkingLot.TimeCreated;
            reqDTO.ParkingLot.TimeModified = DateTime.Now;
            reqDTO.OldParkingLot = oldDTO;
            return reqDTO;
        }

        private RequestDTO ApproveDeactivateType(ParkingLotRequest req, ParkingLot? parkingLotToDeactivate)
        {
            RequestDTO reqDTO;
            parkingLotToDeactivate.IsDeactivated = true;
            parkingLotToDeactivate.TimeModified = DateTime.Now;
            _parkingLotRepository.Update(_mapper.Map<ParkingLot>(parkingLotToDeactivate));
            _requestRepository.Delete(req);
            _unitOfWork.Save();

            var tempDTO = _mapper.Map<TempParkingLotDTO>(parkingLotToDeactivate);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = tempDTO;
            reqDTO.ParkingLot.TimeModified = DateTime.Now;
            return reqDTO;
        }

        private RequestDTO DeclineActivateType(ParkingLotRequest req)
        {
            RequestDTO reqDTO;
            var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId
            && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
            _tempParkingLotRepository.Delete(tempParkingLot);

            _requestRepository.Delete(req);
            _unitOfWork.Save();

            
            var plDTO = _mapper.Map<TempParkingLotDTO>(tempParkingLot);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = plDTO;
            return reqDTO;
        }

        private RequestDTO DeclineUpdateType(ParkingLotRequest req)
        {
            RequestDTO reqDTO;
            var tempParkingLot = _tempParkingLotRepository.GetAsQueryable(p => p.ParkingLotId == req.ParkingLotId
            && p.UserId == req.UserId, null, x => x.Include(y => y.User)).FirstOrDefault();
            _tempParkingLotRepository.Delete(tempParkingLot);

            _requestRepository.Delete(req);
            _unitOfWork.Save();

            var parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == req.ParkingLotId
            && p.UserId == req.UserId).FirstOrDefault();

            var plDTO = _mapper.Map<TempParkingLotDTO>(parkingLot);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = plDTO;
            return reqDTO;
        }

        private RequestDTO DeclineDeactivateType(ParkingLotRequest req, ParkingLot? parkingLotToDeactivate)
        {
            RequestDTO reqDTO;
            _requestRepository.Delete(req);
            _unitOfWork.Save();

            var plDTO = _mapper.Map<TempParkingLotDTO>(parkingLotToDeactivate);
            reqDTO = _mapper.Map<RequestDTO>(req);
            reqDTO.ParkingLot = plDTO;
            return reqDTO;
        }
    }
}
