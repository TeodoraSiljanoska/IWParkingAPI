using AutoMapper;
using Azure;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
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
        private readonly AllCitiesResponse _getResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper _mapper;

        public CityService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _cityRepository = _unitOfWork.GetGenericRepository<City>();
            _cityResponse = new CityResponse();
            _getResponse = new AllCitiesResponse();
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

                _cityResponse.City = createdCity;
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

        public AllCitiesResponse GetAllCities()
        {
            try
            {
                var cities = _cityRepository.GetAll();
                if (cities.Count() == 0)
                {
                    _getResponse.StatusCode = HttpStatusCode.NoContent;
                    _getResponse.Message = "There aren't any cities.";
                    _getResponse.Cities = Enumerable.Empty<City>();
                    return _getResponse;
                }
                _getResponse.Cities = cities;
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Cities returned successfully";
                return _getResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Cities {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Cities");
            }
        }

        public CityResponse GetCityById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("City Id is required");
                }

                City city = _cityRepository.GetById(id);
                if (city == null)
                {
                    throw new NotFoundException("City not found");
                }

                _cityResponse.City = city;
                _cityResponse.StatusCode = HttpStatusCode.OK;
                _cityResponse.Message = "City returned successfully";
                return _cityResponse;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetCityById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetCityById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting City by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the City by Id");
            }
        }

        public CityResponse UpdateCity(int id, CityRequest changes)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("City Id is required");
                }

                City city = _cityRepository.GetById(id);

                if (city == null)
                {
                    throw new NotFoundException("City not found");
                }
                if (changes.Name == city.Name)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                if (changes.Name != city.Name)
                {
                    if (_cityRepository.FindByPredicate(u => u.Name == changes.Name))
                    {
                        throw new BadRequestException("City with that name already exists");
                    }
                }

                city.Name = (city.Name == changes.Name) ? city.Name : changes.Name;

                _cityRepository.Update(city);
                _unitOfWork.Save();

                _cityResponse.City = city;
                _cityResponse.StatusCode = HttpStatusCode.OK;
                _cityResponse.Message = "Zone updated successfully";

                return _cityResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for UpdateCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for UpdateCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while updating the City {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while updating the City");
            }
        }

        public CityResponse DeleteCity(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("City Id is required");
                }

                City city = _cityRepository.GetById(id);
                if (city == null)
                {
                    throw new NotFoundException("City not found");
                }

                _cityRepository.Delete(city);
                _unitOfWork.Save();

                _cityResponse.City = city;
                _cityResponse.StatusCode = HttpStatusCode.OK;
                _cityResponse.Message = "CIty deleted successfully";

                return _cityResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for DeleteCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for DeleteCity {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while deleting the City {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while deleting the City");
            }
        }
    }
}
