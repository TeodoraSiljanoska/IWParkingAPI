CREATE TABLE [Vehicle] (
  [Id] int IDENTITY(1,1),
  [Plate_Number] nvarchar(8) NOT NULL,
  [Type] nvarchar(20) NOT NULL, 
  [User_Id] int NOT NULL,
  PRIMARY KEY ([Id]),
  CONSTRAINT [FK_Vehicle.User_Id]
    FOREIGN KEY ([User_Id])
      REFERENCES [AspNetUsers]([Id])
);