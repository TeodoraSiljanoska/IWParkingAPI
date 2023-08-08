using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using System.Net;
using static IWParkingAPI.Models.Data.EnumClass;

namespace IWParkingAPI.Services.Implementation
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly GetParkingLotsResponse _getResponse;
        private readonly ParkingLotResponse _response;

        public ParkingLotService(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _getResponse = new GetParkingLotsResponse();
            _response = new ParkingLotResponse();
        }
        public GetParkingLotsResponse GetAllParkingLots()
        {
            var parkingLots = _parkingLotRepository.GetAsQueryable(x => x.Status == ((int)StatusEnum.Approved)).ToList();
            if (parkingLots.Count() == 0)
            {
                _getResponse.StatusCode = HttpStatusCode.NoContent;
                _getResponse.Message = "There aren't any parking lots.";
                _getResponse.ParkingLots = Enumerable.Empty<ParkingLot>();
                return _getResponse;
            }
            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Parking lots returned successfully";
            _getResponse.ParkingLots = parkingLots;
            return _getResponse;
        }

        public ParkingLotResponse GetParkingLotById(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Parking Lot Id is required.";
                return _response;

            }

           ParkingLot parkingLot = _parkingLotRepository.GetById(id);

            if (parkingLot == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Parking Lot not found";
                return _response;
            }

            _response.ParkingLot = parkingLot;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Parking Lot returned successfully";
            return _response;
        }

        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            ParkingLot parkingLot = _parkingLotRepository.GetById(id)
;
            if (parkingLot == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Parking lot not found";
                return _response;
            }

            if (parkingLot.IsDeactivated == true)
            {
                _response.StatusCode = HttpStatusCode.NotModified;
                _response.Message = "Parking lot is already deactivated";
                _response.ParkingLot = parkingLot;
                return _response;
            }

            parkingLot.IsDeactivated = true;
            _parkingLotRepository.Update(parkingLot);
            _unitOfWork.Save();

            _response.ParkingLot = parkingLot;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Parking lot deactivated successfully";

            return _response;
        }

    }
}
