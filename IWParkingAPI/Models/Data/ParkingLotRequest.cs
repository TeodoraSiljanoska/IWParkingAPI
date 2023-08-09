using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models.Data;

public partial class ParkingLotRequest
{
    public int Id { get; set; }

    public int Status { get; set; }

    public int UserId { get; set; }

    public int ParkingLotId { get; set; }

    public DateTime TimeCreated { get; set; }

    public virtual ParkingLot ParkingLot { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
