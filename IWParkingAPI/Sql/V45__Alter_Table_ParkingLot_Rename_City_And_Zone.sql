ALTER TABLE [ParkingLot] 
ALTER COLUMN [City] INT NOT NULL;

ALTER TABLE [ParkingLot] 
ALTER COLUMN [Zone] INT NOT NULL;

EXEC sp_rename 'ParkingLot.City', 'City_Id', 'COLUMN'
EXEC sp_rename 'ParkingLot.Zone', 'Zone_Id', 'COLUMN'
