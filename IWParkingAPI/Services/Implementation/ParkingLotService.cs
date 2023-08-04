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
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly GetParkingLotsResponse _response;
        private readonly ParkingLotResponse response;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _response = new GetParkingLotsResponse();
            response = new ParkingLotResponse();
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            var parkingLots = _parkingLotRepository.GetAll();
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

        public ParkingLotResponse GetParkingLotById(int id)
        {
            if (id == 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Parking Lot Id is required.";
                return response;

            }

           ParkingLot parkingLot = _parkingLotRepository.GetById(id);

            if (parkingLot == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Parking Lot not found";
                return response;
            }

            response.ParkingLot = parkingLot;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Parking Lot returned successfully";
            return response;
        }

    }
}
