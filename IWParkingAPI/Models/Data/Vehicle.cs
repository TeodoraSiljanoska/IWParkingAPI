using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models.Data;

public partial class Vehicle
{
    public int Id { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime TimeCreated { get; set; }

    public DateTime? TimeModified { get; set; }

    public bool? IsPrimary { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
