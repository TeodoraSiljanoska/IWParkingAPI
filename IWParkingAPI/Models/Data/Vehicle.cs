using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int UserId { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
