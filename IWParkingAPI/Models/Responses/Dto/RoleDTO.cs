namespace IWParkingAPI.Models.Responses.Dto
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        public DateTime TimeModified { get; set; }
    }
}
