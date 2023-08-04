CREATE TABLE [UsersFavouriteParkingLots] (
    [UserId] int NOT NULL,
    [ParkingLotId] int NOT NULL,
    CONSTRAINT [PK_UsersFavouriteParkingLots] PRIMARY KEY ([UserId], [ParkingLotId]),
    CONSTRAINT [FK_UsersFavouriteParkingLots_ParkingLot_ParkingLotId] FOREIGN KEY ([ParkingLotId]) REFERENCES [Parking Lot] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UsersFavouriteParkingLots_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);