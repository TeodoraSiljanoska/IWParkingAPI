CREATE TABLE [Request] (
  [Id] int IDENTITY(1,1),
  [Status] nvarchar(20) NOT NULL,
  [User_Id] int NOT NULL,
  [Parking_Lot_Id] int NOT NULL,
  PRIMARY KEY ([Id]),
  CONSTRAINT [FK_Request.User_Id]
    FOREIGN KEY ([User_Id])
      REFERENCES [AspNetUsers]([Id]),
	CONSTRAINT [FK_Request.Parking_Lot_Id]
    FOREIGN KEY ([Parking_Lot_Id])
      REFERENCES [Parking Lot]([Id])
);