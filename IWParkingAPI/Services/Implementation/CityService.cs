using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using NLog;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class CityService : ICityService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly CityResponse _cityResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper _mapper;

        public CityService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _cityRepository = _unitOfWork.GetGenericRepository<City>();
            _cityResponse = new CityResponse();
            _mapper = MapperConfig.InitializeAutomapper();
        }

        public CityResponse CreateCity(CityRequest request)
        {
            try
            {
                var city = _cityRepository.GetAsQueryable(x => x.Name.Equals(request.Name)).FirstOrDefault();

                if (city != null)
                {
                    throw new BadRequestException("City with that name already exists");
                }

                var createdCity = _mapper.Map<City>(request);

                _cityRepository.Insert(createdCity);
                _unitOfWork.Save();

                var cityDto = _mapper.Map<CityDTO>(createdCity);

                _cityResponse.City = cityDto;
                _cityResponse.StatusCode = HttpStatusCode.OK;
                _cityResponse.Message = "City created successfully";
                return _cityResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CreateCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for CreateCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while creating the City {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while creating the City");
            }
        }
    }
}
