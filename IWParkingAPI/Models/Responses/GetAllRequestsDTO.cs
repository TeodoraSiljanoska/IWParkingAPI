using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetAllRequestsDTO
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public int UserId { get; set; }

        public int ParkingLotId { get; set; }

        public DateTime TimeCreated { get; set; }

        public virtual ParkingLotDTO ParkingLot { get; set; } = null!;
        public virtual UserDTO User { get; set; } = null!;
    }
}
