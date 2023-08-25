ALTER TABLE [ParkingLot]
ADD CONSTRAINT [FK_ParkingLot.City_Id]
FOREIGN KEY ([City_Id])
REFERENCES [City] ([Id]);

ALTER TABLE [ParkingLot]
ADD CONSTRAINT [FK_ParkingLot.Zone_Id]
FOREIGN KEY ([Zone_Id])
REFERENCES [Zone] ([Id]);