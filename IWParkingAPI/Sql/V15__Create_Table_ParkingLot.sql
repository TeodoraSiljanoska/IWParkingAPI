CREATE TABLE [Parking Lot] (
  [Id] int IDENTITY(1,1),
  [Name] nvarchar(20) NOT NULL,
  [City] nvarchar(20) NOT NULL,
  [Zone] nvarchar(20) NOT NULL,
  [Address] nvarchar(20) NOT NULL,
  [Working_Hour_From] DateTime NOT NULL,
  [Working_Hour_To] Time NOT NULL,
  [Capacity_Car] int NOT NULL,
  [Capacity_Adapted_Car] int NOT NULL,
  [Price] int NOT NULL,
  [User_Id] int NOT NULL,
  PRIMARY KEY ([Id]),
  CONSTRAINT [FK_Parking Lot.User_Id]
    FOREIGN KEY ([User_Id])
      REFERENCES [AspNetUsers]([Id])
);