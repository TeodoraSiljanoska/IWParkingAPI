CREATE TABLE [Payment] (
  [Id] int IDENTITY(1,1),
  [Made_On] datetime NOT NULL,
  [Type] nvarchar(20) NOT NULL,
  [Reservation_Id] int NOT NULL,
  PRIMARY KEY ([Id]),
  CONSTRAINT [FK_Payment.Reservation_Id]
    FOREIGN KEY ([Reservation_Id])
      REFERENCES [Reservation]([Id])
);

