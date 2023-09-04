ALTER TABLE [Reservation]
ADD [Vehicle_Id] INT NOT NULL;

ALTER TABLE [dbo].[Reservation]  WITH CHECK ADD  CONSTRAINT [FK_Reservation.Vehicle_Id] FOREIGN KEY([Vehicle_Id])
REFERENCES [dbo].[Vehicle] ([Id])
GO