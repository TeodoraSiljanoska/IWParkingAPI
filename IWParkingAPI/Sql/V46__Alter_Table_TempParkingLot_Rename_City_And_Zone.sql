ALTER TABLE [TempParkingLot] 
ALTER COLUMN [City] INT NOT NULL;

ALTER TABLE [TempParkingLot] 
ALTER COLUMN [Zone] INT NOT NULL;

EXEC sp_rename 'TempParkingLot.City', 'City_Id', 'COLUMN'
EXEC sp_rename 'TempParkingLot.Zone', 'Zone_Id', 'COLUMN'