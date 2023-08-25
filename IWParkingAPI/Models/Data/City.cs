using System;
using System.Collections.Generic;

namespace IWParkingAPI.Models.Data;

public partial class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ParkingLot> ParkingLots { get; set; } = new List<ParkingLot>();

    public virtual ICollection<TempParkingLot> TempParkingLots { get; set; } = new List<TempParkingLot>();
}
