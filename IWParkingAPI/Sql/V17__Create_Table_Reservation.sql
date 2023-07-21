CREATE TABLE [Reservation] (
  [Id] int IDENTITY(1,1),
  [Type] nvarchar(30) NOT NULL,
  [Start_Date] Date NOT NULL,
  [Start_Time] Time NOT NULL,
  [End_Date] Date NOT NULL,
  [End_Time] Time NOT NULL,
  [Amount] int NOT NULL,
  [Is_Paid] bit NOT NULL,
  [User_Id] int NOT NULL,
  [Parking_Lot_Id] int NOT NULL,
  PRIMARY KEY ([Id]),
  CONSTRAINT [FK_Reservation.User_Id]
    FOREIGN KEY ([User_Id])
      REFERENCES [AspNetUsers]([Id]),
  CONSTRAINT [FK_Reservation.Parking_Lot_Id]
    FOREIGN KEY ([Parking_Lot_Id])
      REFERENCES [Parking Lot]([Id])
);