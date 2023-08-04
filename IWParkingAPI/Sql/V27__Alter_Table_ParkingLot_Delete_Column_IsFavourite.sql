USE [ParkingDB]
GO
ALTER TABLE [dbo].[Parking Lot] DROP CONSTRAINT [DF__Parking L__IsFav__208CD6FA]
GO
ALTER TABLE [dbo].[Parking Lot] DROP COLUMN [IsFavourite]
GO
