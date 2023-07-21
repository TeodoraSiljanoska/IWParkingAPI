using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models;

public partial class Request
{
    public int Id { get; set; }

    public string Status { get; set; } = null!;

    public int UserId { get; set; }

    public int ParkingLotId { get; set; }

    public virtual ParkingLot ParkingLot { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
