﻿using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IParkingLotService
    {
        public GetParkingLotsResponse GetAllParkingLots();
        public ParkingLotResponse GetParkingLotById(int id);
        public ParkingLotResponse DeactivateParkingLot(int id);
        public ParkingLotResponse CreateParkingLot(ParkingLotReq request);
        public ParkingLotResponse UpdateParkingLot(int id, UpdateParkingLotRequest changes);
        public ParkingLotResponse MakeParkingLotFavorite(int parkingLotId);
        public ParkingLotResponse RemoveParkingLotFavourite(int parkingLotId);
        public GetParkingLotsDTOResponse GetUserFavouriteParkingLots();
    }
}
