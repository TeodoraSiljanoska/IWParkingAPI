ALTER TABLE [dbo].[ParkingLotRequest]  WITH CHECK ADD  CONSTRAINT [FK_Request.ParkingLot_Id] FOREIGN KEY([Parking_Lot_Id])
REFERENCES [dbo].[TempParkingLot] ([Id])
GO