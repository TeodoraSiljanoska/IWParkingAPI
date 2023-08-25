ALTER TABLE [TempParkingLot]
ADD CONSTRAINT [FK_TempParkingLot.City_Id]
FOREIGN KEY ([City_Id])
REFERENCES [City] ([Id]);

ALTER TABLE [TempParkingLot]
ADD CONSTRAINT [FK_TempParkingLot.Zone_Id]
FOREIGN KEY ([Zone_Id])
REFERENCES [Zone] ([Id]);

