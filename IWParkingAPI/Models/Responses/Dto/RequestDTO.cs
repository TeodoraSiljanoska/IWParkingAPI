using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Models.Responses.Dto
{
    public class RequestDTO
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public int UserId { get; set; }
        public string Type { get; set; }

        public int ParkingLotId { get; set; }

        public DateTime TimeCreated { get; set; }

        public TempParkingLotDTO ParkingLot { get; set; }

        public virtual UserDTO User { get; set; } = null!;
    }
}
