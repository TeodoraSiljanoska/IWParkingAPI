namespace IWParkingAPI.Models.Responses.Dto
{
    public class VehicleDTO
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string Type { get; set; } = null!;

        public int UserId { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime? TimeModified { get; set; }

        public bool? IsPrimary { get; set; }

        public virtual UserWithoutRoleDTO User { get; set; } = null!;
    }
}
