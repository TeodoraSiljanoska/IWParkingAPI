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
using NLog;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class ZoneService : IZoneService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<Zone> _zoneRepository;
        private readonly ZoneResponse _response;
        private readonly AllZonesResponse _getResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public ZoneService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _zoneRepository = _unitOfWork.GetGenericRepository<Zone>();
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new ZoneResponse();
            _getResponse = new AllZonesResponse();
        }
        public AllZonesResponse GetAllZones()
        {
            try
            {
                var zones = _zoneRepository.GetAll();
                if (zones.Count() == 0)
                {
                    _getResponse.StatusCode = HttpStatusCode.NoContent;
                    _getResponse.Message = "There aren't any zones.";
                    _getResponse.Zones = Enumerable.Empty<Zone>();
                    return _getResponse;
                }

                _getResponse.Zones = zones;
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Zones returned successfully";
                return _getResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Zones {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Zones");
            }
        }
        public ZoneResponse GetZoneById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Zone Id is required");
                }

                Zone zone = _zoneRepository.GetById(id);
                if (zone == null)
                {
                    throw new NotFoundException("Zone not found");
                }

                _response.Zone = zone;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Zone returned successfully";
                return _response;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetZoneById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetZoneById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting Zone by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Zone by Id");
            }
        }

        public ZoneResponse CreateZone(ZoneRequest request)
        {
            try
            {
                if (_zoneRepository.FindByPredicate(u => u.Name == request.Name))
                {
                    throw new BadRequestException("Zone already exists");
                }

                var zone = _mapper.Map<Zone>(request);

                _zoneRepository.Insert(zone);
                _unitOfWork.Save();

                _response.Zone = zone;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Zone created successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CreateRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while creating the Role {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while creating the Role");
            }
        }
        public ZoneResponse UpdateZone(int id, ZoneRequest changes)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Zone Id is required");
                }

                Zone zone = _zoneRepository.GetById(id);

                if (zone == null)
                {
                    throw new NotFoundException("Zone not found");
                }
                if (changes.Name == zone.Name)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                if (changes.Name != zone.Name)
                {
                    if (_zoneRepository.FindByPredicate(u => u.Name == changes.Name))
                    {
                        throw new BadRequestException("Zone with that name already exists");
                    }
                }

                zone.Name = (zone.Name == changes.Name) ? zone.Name : changes.Name;

                _zoneRepository.Update(zone);
                _unitOfWork.Save();


                _response.Zone = zone;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Zone updated successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for UpdateZone {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for UpdateZone {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while updating the Zone {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while updating the Zone");
            }
        }

        public ZoneResponse DeleteZone(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Zone Id is required");
                }

                Zone zone = _zoneRepository.GetById(id);
                if (zone == null)
                {
                    throw new NotFoundException("Zone not found");
                }

                _zoneRepository.Delete(zone);
                _unitOfWork.Save();

                _response.Zone = zone;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Zone deleted successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for DeleteZone {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for DeleteZone {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while deleting the Zone {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while deleting the Zone");
            }
        }
    }
}
