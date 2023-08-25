using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models.Data;

public partial class TempParkingLot
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CityId { get; set; }

    public int ZoneId { get; set; }

    public string Address { get; set; } = null!;

    public TimeSpan WorkingHourFrom { get; set; }

    public TimeSpan WorkingHourTo { get; set; }

    public int CapacityCar { get; set; }

    public int CapacityAdaptedCar { get; set; }

    public int Price { get; set; }

    public int UserId { get; set; }

    public bool? IsDeactivated { get; set; }

    public DateTime TimeCreated { get; set; }

    public DateTime? TimeModified { get; set; }

    public int Status { get; set; }

    public int? ParkingLotId { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual Zone Zone { get; set; } = null!;
}
