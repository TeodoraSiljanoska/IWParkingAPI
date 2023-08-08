namespace IWParkingAPI.Models.Responses
{
    public class RequestDTO
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;

        public int UserId { get; set; }

        public int ParkingLotId { get; set; }

        public DateTime TimeCreated { get; set; }
    }
}
