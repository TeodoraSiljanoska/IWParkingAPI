using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingPlotRepository;
        private readonly GetParkingLotsResponse _response;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _parkingPlotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _response = new GetParkingLotsResponse();
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            var parkingLots = _parkingPlotRepository.GetAll();
            if (parkingLots.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.Message = "There aren't any parking lots.";
                _response.ParkingLots = Enumerable.Empty<ParkingLot>();
                return _response;
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Parking lots returned successfully";
            _response.ParkingLots = parkingLots;
            return _response;
        }
    }
}
